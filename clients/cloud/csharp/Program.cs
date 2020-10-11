// Copyright 2020 Confluent Inc.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ElectronicFundTransfer;
using ElectronicFundTransfer.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace CCloud
{
    class Program
    {
        static async Task<ProducerConfig> LoadConfig()
        {
            try
            {
                var path = Directory.GetCurrentDirectory();
                var brokerList = "k8s-01-kafka.dev.be.ia.iafg.net:31090,k8s-01-kafka.dev.be.ia.iafg.net:31091,k8s-01-kafka.dev.be.ia.iafg.net:31092";
                var config = new ProducerConfig
                {
                    BootstrapServers = brokerList,
                    ClientId = Dns.GetHostName(),
                    LingerMs = 5
                };
                config.SecurityProtocol = SecurityProtocol.Ssl;

                // wrong
                config.SslCaLocation = $"{path}/ssl/DS.SalVag-PmtLoadingService.crt";
                // correct
                config.SslCaLocation = $"{path}/ssl/DEV_ia-kafka-dev-ca.crt";
                config.SslKeystoreLocation = $"{path}/ssl/DS.SalVag-PmtLoadingService.keystore.jks";
                config.SslKeystorePassword = "HW0jNxCx9YQ47pycVJ4UrQIIKxxFZ2DJ";

                // Unifi2WebService
                /*
                config.SslCaLocation = $"{path}/ssl/DEV_ia-kafka-dev-ca.crt";
                config.SslKeystoreLocation = $"{path}/ssl/DS.Unifi2WebService.keystore.jks";
                config.SslKeystorePassword = "U8qEVYzOuwKGngJmvpZWuz93g9GBDokt";
                */

                // works for DS_Dealer_Event_ASMB topic
                /*
                config.SslCaLocation = $"{path}/ssl/DEV_ia-kafka-dev-ca.crt";
                config.SslKeystoreLocation = $"{path}/ssl/DEV_DS.UnificationWebService.keystore.jks";
                config.SslKeystorePassword = "JC9kfGwvyxkcBchwnP3KwKqFxEDcjWrY";
                */

                config.EnableIdempotence = true;

                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured {e.Message}");
                System.Environment.Exit(1);
                return null; // avoid not-all-paths-return-value compiler error.
            }
        }

        static async Task CreateTopicMaybe(string name, int numPartitions, short replicationFactor, ClientConfig cloudConfig)
        {
            using (var adminClient = new AdminClientBuilder(cloudConfig).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                        new TopicSpecification { Name = name, NumPartitions = numPartitions, ReplicationFactor = replicationFactor } });
                }
                catch (CreateTopicsException e)
                {
                    if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                    {
                        Console.WriteLine($"An error occured creating topic {name}: {e.Results[0].Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine("Topic already exists");
                    }
                }
            }
        }

        static async Task ProduceAsync(string topic, ProducerConfig config)
        {
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                int numProduced = 0;
                int numMessages = 1;
                for (int i = 0; i < numMessages; ++i)
                {
                    var msg = new BankResultMessage
                    {
                        Id = Guid.NewGuid(),
                        Specversion = "1.0",
                        Type = BankResultType.PaymentSucceeded,
                        Version = "1.0",
                        Subject = "paymentid-999999",
                        Time = DateTime.Now,
                        Source = SourceSystem.UniFi,
                        DataContentType = DataContentType.JSON,
                        CorrelationId = Guid.NewGuid(),
                        Topic = BankResultTopic.Priv_SAL_Product_BankResultSuccessUnifi_Event,
                        Env = EnvType.ASMB,
                        data = new BankResultMessageBody
                        {
                            Context = "Bank Result Payment CMF",
                            BankReportDate = DateTime.Now,
                            BankReportName = "RBC",
                            PaymentMethod = PaymentMethod.EFT,
                            LanguageCode = "E",
                            BankPaymentSuccessful = true,
                            Currency = "CAD",
                            CompanyCode = "IA",
                            CustomerAccount = "20303",
                            AuthorizationNumber = "77658",
                            ContractDealerNumber = "BC999999",
                            DepositDate = DateTime.Now,
                            Amount = 12345.09,
                            EftDetails = new PaymentEftDetails
                            {
                                BankNumber = "27272727",
                                Branch = "23089",
                                BankAccount = "33333",
                                BeneficiaryName = "John Smith"
                            }
                        }
                        
                    };

                    var key = "alice";
                    var val = JObject.FromObject(msg).ToString(Formatting.None);

                    Console.WriteLine($"Producing record: {key} {val}");

                    var dr = await producer.ProduceAsync(topic, new Message<string, string> { Value = val });

                    Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");

                }

                producer.Flush(TimeSpan.FromSeconds(10));

                Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
            }
        }

        static void Consume(string topic, ClientConfig config)
        {
            var consumerConfig = new ConsumerConfig(config);
            consumerConfig.GroupId = "dotnet-example-group-1";
            consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
            consumerConfig.EnableAutoCommit = false;

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
            {
                consumer.Subscribe(topic);
                var totalCount = 0;
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        totalCount += JObject.Parse(cr.Message.Value).Value<int>("count");
                        Console.WriteLine($"Consumed record with key {cr.Message.Key} and value {cr.Message.Value}, and updated total count to {totalCount}");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("usage: .. produce|consume <topic> <configPath> [<certDir>]");
            System.Environment.Exit(1);
        }

        static async Task Main(string[] args)
        {
            var config = await LoadConfig();
            var mode = "produce";
            // var topic = "DS_StatementReadyToFinilize_ASMB";
            var topic = "Priv_DS_Product_BankResultSuccessUnifi_Event_ASMB";
            // var topic = "DS_Dealer_Event_ASMB";

            switch (mode)
            {
                case "produce":
                    // await CreateTopicMaybe(topic, 1, 3, config);
                    await ProduceAsync(topic, config);
                    break;
                case "consume":
                    Consume(topic, config);
                    break;
                default:
                    PrintUsage();
                    break;
            }
        }
    }
}

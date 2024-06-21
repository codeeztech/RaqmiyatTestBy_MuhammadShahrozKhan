using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using FileProcessing.WebAPI;
using FileProcessing.WebAPI.Data;
using FileProcessing.WebAPI.Models;
using FileProcessing.WebAPI.SeedData;
using Xunit;

namespace FileProcessing.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HttpClient Client { get; private set; }

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            SetUpClient();
        }

        // TEST NAME - checkMiddleware
        // TEST DESCRIPTION - Forbidden if there is no token in request
        [Fact]
        public async Task Test_TestMiddleWareToken_403()
        {
            var response0 = await Client.GetAsync("/api/testmiddleware/token");
            response0.StatusCode.Should().BeEquivalentTo(403);
        }

        // TEST NAME - checkMiddleware
        // TEST DESCRIPTION - allow if there is no token in request
        [Fact]
        public async Task Test_TestMiddleWareNoToken_200()
        {
            var response1 = await Client.GetAsync("/api/testmiddleware/notoken");
            response1.StatusCode.Should().BeEquivalentTo(200);
        }

        // TEST NAME - checkMiddleware
        // TEST DESCRIPTION - Allow if there is token 12345678 in request
        [Fact]
        public async Task Test_TestMiddleWareToken_200()
        {
            var response2 = await Client.GetAsync("/api/testmiddleware/token/?token=12345678");
            response2.StatusCode.Should().BeEquivalentTo(200);
        }

        // TEST NAME - analyzeFile
        // TEST DESCRIPTION - In this test User should send byte array to the web api and get analytic
        [Fact]
        public async Task Test_FileAnalyze_200()
        {
            //Here data is exporting to the end point
            var myJsonString = File.ReadAllBytes("UserCollection.xml");
            var content = new ByteArrayContent(myJsonString);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            var response0 = await Client.PostAsync("/api/file/analyze", content);
            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var statisticalData = JsonConvert.DeserializeObject<StatisticalModel>(response0.Content.ReadAsStringAsync().Result);

            statisticalData.MinRate.Should().Be(10);
            statisticalData.MaxRate.Should().Be(60);
            statisticalData.AverageRate.Should().Be((float)35.828);
        }

        // TEST NAME - addUserToFile
        // TEST DESCRIPTION - Add users list in file.
        [Fact]
        public async Task Test_AddUserToFile_200()
        {
            //Here data is exporting to the end point
            var myJsonString = File.ReadAllBytes("UserCollection.xml");
            var user1 = new UserForAddModelForm { Id = 1001, Email = "test1@mail.com", FirstName = "Denny", LastName = "Chadwyck", Rate = 8 };
            var user2 = new UserForAddModelForm { Id = 1002, Email = "test2@mail.com", FirstName = "Meredithe", LastName = "Vannet", Rate = 87 };
            var user3 = new UserForAddModelForm { Id = 1003, Email = "test3@mail.com", FirstName = "Cymbre", LastName = "Spini", Rate = 90 };

            var modelForAddUsers = new AddUsersToFileModelForm
            {
                Content = Convert.ToBase64String(myJsonString),
                Users = new List<UserForAddModelForm> { user1, user2, user3 }
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(modelForAddUsers), Encoding.UTF8, "application/json");

            var response0 = await Client.PostAsync("/api/file/adduser", stringContent);

            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var resultXML = response0.Content.ReadAsStringAsync().Result;
            var users = ParseUsersFromXml(resultXML);

            users.Count.Should().Be(1003);
            users.FirstOrDefault(x => x.Email == "test1@mail.com" && x.FirstName == "Denny").Should().NotBeNull();
            users.FirstOrDefault(x => x.Email == "test2@mail.com" && x.FirstName == "Meredithe").Should().NotBeNull();
            users.FirstOrDefault(x => x.Email == "test3@mail.com" && x.FirstName == "Cymbre").Should().NotBeNull();
        }

        // TEST NAME - addUserToFile
        // TEST DESCRIPTION - add users in file and check the statistic
        [Fact]
        public async Task Test_AddUserToFile_200_Statistics()
        {
            //Here data is exporting to the end point
            var myJsonString = File.ReadAllBytes("UserCollection.xml");
            var user1 = new UserForAddModelForm { Id = 1001, Email = "test1@mail.com", FirstName = "Denny", LastName = "Chadwyck", Rate = 8 };
            var user2 = new UserForAddModelForm { Id = 1002, Email = "test2@mail.com", FirstName = "Meredithe", LastName = "Vannet", Rate = 87 };
            var user3 = new UserForAddModelForm { Id = 1003, Email = "test3@mail.com", FirstName = "Cymbre", LastName = "Spini", Rate = 90 };

            var modelForAddUsers = new AddUsersToFileModelForm
            {
                Content = Convert.ToBase64String(myJsonString),
                Users = new List<UserForAddModelForm> { user1, user2, user3 }
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(modelForAddUsers), Encoding.UTF8, "application/json");

            var response0 = await Client.PostAsync("/api/file/adduser", stringContent);

            response0.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);
            var resultXML = response0.Content.ReadAsStringAsync().Result;
            var users = ParseUsersFromXml(resultXML);

            users.Count.Should().Be(1003);
            users.FirstOrDefault(x => x.Email == "test1@mail.com" && x.FirstName == "Denny").Should().NotBeNull();
            users.FirstOrDefault(x => x.Email == "test2@mail.com" && x.FirstName == "Meredithe").Should().NotBeNull();
            users.FirstOrDefault(x => x.Email == "test3@mail.com" && x.FirstName == "Cymbre").Should().NotBeNull();

            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(resultXML));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            var response1 = await Client.PostAsync("/api/file/analyze", content);
            response1.StatusCode.Should().BeEquivalentTo(StatusCodes.Status200OK);

            var statisticalData = JsonConvert.DeserializeObject<StatisticalModel>(response1.Content.ReadAsStringAsync().Result);
            statisticalData.MinRate.Should().Be(8);
            statisticalData.MaxRate.Should().Be(90);
            statisticalData.AverageRate.Should().Be((float)35.9052849);
        }

        private static List<XMLUser> ParseUsersFromXml(string resultXML)
        {
            var users = new List<XMLUser>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(resultXML);
            var userCollection = xmlDoc.SelectSingleNode("UserCollection");
            var usersNodes = userCollection.SelectNodes("User");
            foreach (XmlElement xmlElement in usersNodes)
            {
                users.Add(new XMLUser
                {
                    Email = xmlElement.SelectSingleNode("email").InnerText,
                    FirstName = xmlElement.SelectSingleNode("first_name").InnerText,
                    LastName = xmlElement.SelectSingleNode("last_name").InnerText,
                    Rate = Convert.ToInt32(xmlElement.SelectSingleNode("rate").InnerText)
                });
            }

            return users;
        }

        private void SetUpClient()
        {
            Client = _factory.WithWebHostBuilder(builder =>
                builder.UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var context = new FileProcessingContext(new DbContextOptionsBuilder<FileProcessingContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);

                    services.RemoveAll(typeof(FileProcessingContext));
                    services.AddSingleton(context);

                    context.Database.OpenConnection();
                    context.Database.EnsureCreated();

                    context.SaveChanges();

                    // Clear local context cache
                    foreach (var entity in context.ChangeTracker.Entries().ToList())
                    {
                        entity.State = EntityState.Detached;
                    }
                })).CreateClient();
        }
    }
}

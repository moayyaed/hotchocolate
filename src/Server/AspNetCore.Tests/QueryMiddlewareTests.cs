using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChilliCream.Testing;
using HotChocolate.Types;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.AspNetCore
{
    public class QueryMiddlewareTests
        : IClassFixture<TestServerFactory>
    {
        public QueryMiddlewareTests(TestServerFactory testServerFactory)
        {
            TestServerFactory = testServerFactory;
        }

        private TestServerFactory TestServerFactory { get; set; }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task HttpPost_BasicTest(bool multipart)
        {
            // arrange
            TestServer server = CreateTestServer("/foo");
            var request = new ClientQueryRequest { Query = "{ basic { a } }" };

            // act
            HttpResponseMessage message;

            if (multipart)
            {
                message = await server.SendRequestAsync(request, "foo");
            }
            else
            {
                message = await server.SendMultipartRequestAsync(request, "foo");
            }

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_Casing()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = "{ A:basic { B:a } }"
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_EnumArgument()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = "query a($a: TestEnum) { withEnum(test: $a) }",
                Variables = JObject.FromObject(new Dictionary<string, object>
                {
                    { "a", "A"}
                })
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_NestedEnumArgument()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = "query a($a: BarInput) { withNestedEnum(bar: $a) }",
                Variables = JObject.FromObject(new Dictionary<string, object>
                {
                    { "a",  new Dictionary<string, object>
                            {
                                { "a",  "B" }
                            }
                    }
                })
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpGet_BasicTest()
        {
            // arrange
            TestServer server = CreateTestServer();
            var query = "{ basic { a } }";

            // act
            HttpResponseMessage message =
                await server.SendGetRequestAsync(query);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpGet_ForwardToNextMiddleware()
        {
            // arrange
            TestServer server = CreateTestServer();

            // act
            HttpResponseMessage message = await server.CreateClient()
                .GetAsync($"http://localhost:5000/1234");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, message.StatusCode);
        }

        [Fact]
        public async Task HttpPost_WithScalarVariables()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = @"
                query test($a: String!) {
                    withScalarArgument(a: $a) {
                        a
                        b
                        c
                    }
                }",
                Variables = JObject.FromObject(new Dictionary<string, object>
                {
                    { "a", "1234567890"}
                })
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_WithObjectVariables()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = @"
                query test($a: FooInput!) {
                    withObjectArgument(b: $a) {
                        a
                        b
                        c
                    }
                }",
                Variables = JObject.FromObject(new Dictionary<string, object>
                {
                    { "a", new Dictionary<string, object> {
                        {"a", "44"},
                        {"b", "55"},
                        {"c", 66}
                    } }
                })
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_WithScopedService()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = @"
                {
                    sayHello
                }"
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_WithHttpContext()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = @"
                {
                    requestPath
                    requestPath2
                    requestPath3
                }"
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_CustomProperties()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = new ClientQueryRequest
            {
                Query = @"
                {
                    customProperty
                }"
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_GraphQLRequest()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = @"
                {
                    customProperty
                }";
            var contentType = "application/graphql";

            // act
            HttpResponseMessage message =
                await server.SendPostRequestAsync(request, contentType, null);

            // assert
            Assert.Equal(HttpStatusCode.OK, message.StatusCode);

            string json = await message.Content.ReadAsStringAsync();
            ClientQueryResult result = JsonConvert
                .DeserializeObject<ClientQueryResult>(json);
            Assert.Null(result.Errors);
            result.MatchSnapshot();
        }

        [Fact]
        public async Task HttpPost_UnknownContentType()
        {
            // arrange
            TestServer server = CreateTestServer();
            var request = @"
                {
                    customProperty
                }";
            var contentType = "application/foo";

            // act
            HttpResponseMessage message =
                await server.SendPostRequestAsync(request, contentType, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, message.StatusCode);
        }

        [InlineData("/", null, HttpStatusCode.OK)]
        [InlineData("/", "/", HttpStatusCode.OK)]
        [InlineData("/graphql", "/graphql/", HttpStatusCode.OK)]
        [InlineData("/graphql", "/graphql", HttpStatusCode.OK)]
        [InlineData("/graphql/", "/graphql", HttpStatusCode.OK)]
        [InlineData("/graphql/", "/graphql/", HttpStatusCode.OK)]
        [InlineData("/graphql", "/graphql/foo", HttpStatusCode.NotFound)]
        [InlineData("/graphql/foo", "/graphql/foo/", HttpStatusCode.OK)]
        [InlineData("/graphql/foo", "/graphql/foo", HttpStatusCode.OK)]
        [Theory]
        public async Task HttpPost_Path(
            string path,
            string requestPath,
            HttpStatusCode httpStatus)
        {
            // arrange
            TestServer server = CreateTestServer(path);
            var request = new ClientQueryRequest
            {
                Query = @"
                {
                    customProperty
                }"
            };

            // act
            HttpResponseMessage message =
                await server.SendRequestAsync(request, requestPath);

            // assert
            Assert.Equal(httpStatus, message.StatusCode);

            if (message.StatusCode == HttpStatusCode.OK)
            {
                string result = await message.Content.ReadAsStringAsync();
                result.MatchSnapshot(new SnapshotNameExtension(
                    path?.Replace("/", "_Slash_"),
                    requestPath?.Replace("/", "_Slash_")));
            }
        }

        private TestServer CreateTestServer(string path = null)
        {
            return TestServerFactory.Create(
                c =>
                {
                    c.RegisterQueryType<QueryType>();
                    c.RegisterType<InputObjectType<Bar>>();
                },
                new QueryMiddlewareOptions
                {
                    Path = path ?? "/",
                    OnCreateRequest = (context, request, ct) =>
                    {
                        request.AddProperty("foo", "bar");
                        return Task.CompletedTask;
                    }
                });
        }
    }
}

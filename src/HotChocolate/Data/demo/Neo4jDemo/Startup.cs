using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Data.Neo4J.Execution;
using HotChocolate.Data.Neo4J.Extensions;
using HotChocolate.Data.Neo4J.Language;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotChocolate.Types;
using Neo4j.Driver;

namespace Neo4jDemo
{
    public class Startup
    {
        [Neo4JNode("Movie", "Film")]
        public class Movie
        {
            public string Title { get; set; }
            public int? Released { get; set; }
            public string Tagline { get; set; }
            public double? ImdbRating { get; set; }

            [Neo4JRelationship("ACTED_IN", RelationshipDirection.Incoming)]
            public List<Actor> Actors { get; set; }
        }

        public class Car
        {
            public string Name { get; set; }
        }

        public class Actor
        {
            public string Name { get; set; }

            [Neo4JRelationship("ACTED_IN", RelationshipDirection.Outgoing)]
            public List<Movie> ActedIn { get; set; }
        }


        [ExtendObjectType(Name = "Query")]
        public class Query
        {
            [UseNeo4JDatabase("neo4j")]
            [UseFiltering]
            public async Task<List<Movie>> GetMovies([ScopedService] IAsyncSession session)
            {
                IResultCursor cursor = await session.RunAsync(@"
                        MATCH (m:Movie)
                        RETURN m
                    ");

                return await cursor.MapAsync<Movie>();
            }

            [UseNeo4JDatabase("movies")]
            [UseFiltering]
            public Neo4JExecutable<Movie> GetMovie() => default;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(
                    //DbClient.CreateDriver("bolt://localhost:7687"))
                    DbClient.CreateDriver("bolt://localhost:7687", "neo4j", "test123"))
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                    .AddType<Query>()
                .AddNeo4JFiltering();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL("/");
            });
        }
    }
}

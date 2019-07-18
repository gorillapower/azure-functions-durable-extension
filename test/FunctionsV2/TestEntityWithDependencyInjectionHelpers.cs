// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Tests
{
    public static class TestEntityWithDependencyInjectionHelpers
    {
        public const string DummyEnvironmentVariable = "DUMMY_ENVIRONMENT_VARIABLE";
        public const string DummyEnvironmentVariableValue = "DUMMY_VALUE";

        public interface IEnvironment
        {
            Task<string> GetEnvironmentVariable(string variableName);
        }

        public static async Task<string> EnvironmentOrchestration([OrchestrationTrigger] IDurableOrchestrationContext ctx)
        {
            var environment = ctx.GetInput<EntityId>();

            var entityProxy = ctx.CreateEntityProxy<IEnvironment>(environment);

            // get current value
            return await entityProxy.GetEnvironmentVariable(DummyEnvironmentVariable);
        }

        [FunctionName(nameof(Environment))]
        public static Task CounterFunction([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Environment>();
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class Environment : IEnvironment
        {
            private readonly INameResolver nameResolver;

            public Environment(INameResolver nameResolver)
            {
                this.nameResolver = nameResolver;
            }

            public Task<string> GetEnvironmentVariable(string variableName)
            {
                return Task.FromResult(this.nameResolver.Resolve(variableName));
            }

            public void Delete()
            {
                Entity.Current.DestructOnExit();
            }
        }

        public class DummyEnvironmentVariableResolver : INameResolver
        {
            public string Resolve(string name)
            {
                if (string.Equals(name, DummyEnvironmentVariable))
                {
                    return DummyEnvironmentVariableValue;
                }

                return null;
            }
        }
    }
}

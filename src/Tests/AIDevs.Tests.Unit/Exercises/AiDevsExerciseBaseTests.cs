﻿using AIDevs.Shared.Infrastructure.OpenAi;
using Xunit.Abstractions;

namespace AIDevs.Tests.Unit.Exercises
{
    public class AiDevsExerciseBaseTests
    {
        public AiDevsExerciseBaseTests(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            ExercisesClient = AiDevsExercisesClientFactory.Create();
            OpenAiClient = OpenAiClientFactory.Create();
        }

        public ITestOutputHelper TestOutputHelper { get; }
        public AiDevsExercisesClient ExercisesClient { get; }
        public OpenAiClient OpenAiClient { get; }
    }
}
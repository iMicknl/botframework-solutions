﻿using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Solutions.Shared.Authentication;
using Microsoft.Bot.Configuration;
using System.Collections.Generic;

namespace VirtualAssistantTemplate.Services
{
    public class BotSettings
    {
        public string MicrosoftAppId { get; set; }

        public string MicrosoftAppPassword { get; set; }

        public List<OAuthConnection> OAuthConnections { get; set; }

        public string DefaultLocale { get; set; }

        public CosmosDbStorageOptions CosmosDb { get; set; }

        public TelemetryConfiguration AppInsights { get; set; }

        public BlobStorageConfiguration BlobStorage { get; set; }

        public ContentModeratorConfiguration ContentModerator { get; set; }

        public Dictionary<string, CognitiveModelConfiguration> CognitiveModels { get; set; }

        public List<SkillDefinition> Skills { get; set; }

        public class BlobStorageConfiguration
        {
            public string ConnectionString { get; set; }
            public string Container { get; set; }
        }

        public class ContentModeratorConfiguration
        {
            public string Key { get; set; }
        }

        public class CognitiveModelConfiguration
        {
            public DispatchService DispatchModel { get; set; }

            public List<LuisService> LanguageModels { get; set; }

            public List<QnAMakerService> Knowledgebases { get; set; }
        }
    }
}
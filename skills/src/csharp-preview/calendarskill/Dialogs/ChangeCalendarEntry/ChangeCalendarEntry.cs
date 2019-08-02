﻿using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;

/// <summary>
/// This dialog will accept all the calendar entris if they have the same subject
/// </summary>
namespace Microsoft.CalendarSample
{
    public class ChangeCalendarEntry : ComponentDialog
    {
        private static IConfiguration Configuration;
        public ChangeCalendarEntry(IConfiguration configuration)
            : base(nameof(ChangeCalendarEntry))
        {
            Configuration = configuration;
            // Create instance of adaptive dialog. 
            var acceptCalendarEntry = new AdaptiveDialog("change")
            {
                Recognizer = CreateRecognizer(),
                Generator = new ResourceMultiLanguageGenerator("ChangeCalendarEntry.lg"),
                Steps = new List<IDialog>()
                {
                   
                    new SendActivity("[emptyFocusedMeeting]"),
                    new SetProperty()
                    {
                        Property = "user.ShowAllMeetingDialog_pageIndex",// index must be set to zero
                        Value = "0" // in case we have not entered FindCalendarEntry from RootDialog
                    },
                    new BeginDialog("ShowAllMeetingDialog"),
                    new IfCondition()
                    {
                        Condition = "user.focusedMeeting == null",
                        Steps = new List<IDialog>(){
                            new SendActivity("[EmptyCalendar]"),
                            new EndDialog()
                        }
                    },
                    new ConfirmInput()
                    {
                        Property = "turn.ChangeCalendarEntry_ConfirmChoice",
                        Prompt = new ActivityTemplate("[UpdateConfirm]"),
                        InvalidPrompt = new ActivityTemplate("[YesOrNo]"),
                    },
                    new IfCondition()
                    {
                        Condition = "turn.ChangeCalendarEntry_ConfirmChoice",
                        Steps = new List<IDialog>()
                        {
                            new DateTimeInput()
                            {
                                Property = "dialog.ChangeCalendarEntry_startTime",
                                Prompt = new ActivityTemplate("[GetStartTime]")
                            },
                            new HttpRequest()
                            {
                                Property = "user.updateResponse",
                                Method = HttpRequest.HttpMethod.PATCH,
                                Url = "https://graph.microsoft.com/v1.0/me/events/{user.focusedMeeting.id}",
                                Headers =  new Dictionary<string, string>()
                                {
                                    ["Authorization"] = "Bearer {user.token.Token}",
                                },
                                Body = JObject.Parse(@"{
                                    'start': {
                                        'dateTime': '{formatDateTime(dialog.ChangeCalendarEntry_startTime[0].value, \'yyyy-MM-ddTHH:mm:ss\')}', 
                                        'timeZone': 'UTC'
                                    }
                                }")
                            },
                            new IfCondition()
                            {
                                Condition = "user.updateResponse.error == null",
                                Steps = new List<IDialog>
                                {
                                    new SendActivity("[UpdateCalendarEntryReadBack]")
                                },
                                ElseSteps = new List<IDialog>
                                {
                                    new SendActivity("[UpdateCalendarEntryFailed]")
                                }
                            },
                        }
                    },

                    // we cannot accept a entry if we are the origanizer
                    
                    new SendActivity("[Welcome-Actions]"),
                    new EndDialog()
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("Help")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("[HelpUpdateMeeting]")
                        }
                    },
                    new IntentRule("Cancel")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("[CancelUpdateMeeting]"),
                            new CancelAllDialogs()
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(acceptCalendarEntry);
            acceptCalendarEntry.AddDialog(
                new List<Dialog> {
                    new ShowAllMeetingDialog(Configuration)
                });

            // The initial child Dialog to run.
            InitialDialogId = "change";
        }
        public static IRecognizer CreateRecognizer()
        {
            if (string.IsNullOrEmpty(Configuration["LuisAppIdGeneral"]) || string.IsNullOrEmpty(Configuration["LuisAPIKeyGeneral"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostNameGeneral"]))
            {
                throw new Exception("Your LUIS application is not configured. Please see README.MD to set up a LUIS application.");
            }
            return new LuisRecognizer(new LuisApplication()
            {
                Endpoint = Configuration["LuisAPIHostNameGeneral"],
                EndpointKey = Configuration["LuisAPIKeyGeneral"],
                ApplicationId = Configuration["LuisAppIdGeneral"]
            });
        }
    }
}

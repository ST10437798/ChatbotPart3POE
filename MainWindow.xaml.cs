// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; // For Thread.Sleep if you re-implement TypeWriterEffect in console (not directly in WPF UI thread)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading; // For DispatcherTimer in WPF
using System.Text.RegularExpressions; // Added for Regular Expressions
using System.IO;

namespace EnhancedCybersecurityChatbot
{
    // ---
    // ActivityLogEntry Class
    // ---
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }

        public ActivityLogEntry(string description)
        {
            Timestamp = DateTime.Now;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} - {Description}";
        }
    }

    // ---
    // TaskItem Class
    // ---
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; } // Nullable DateTime for optional reminder
        public bool IsCompleted { get; set; }
        public DateTime DateAdded { get; set; }

        public TaskItem(string title, string description, DateTime? reminderDate = null)
        {
            Title = title;
            Description = description;
            ReminderDate = reminderDate;
            IsCompleted = false; // By default, a new task is not completed
            DateAdded = DateTime.Now;
        }

        public override string ToString()
        {
            string reminderInfo = ReminderDate.HasValue ? $" (Reminder: {ReminderDate.Value:yyyy-MM-dd HH:mm})" : "";
            string status = IsCompleted ? "[COMPLETED]" : "[PENDING]";
            return $"{status} {Title}: {Description}{reminderInfo}";
        }
    }

    // ---
    // CybersecurityTaskManager Class
    // ---
    public class CybersecurityTaskManager
    {
        private List<TaskItem> tasks;
        private Action<string> _logActivity; // Delegate to log activities

        public CybersecurityTaskManager(Action<string> logActivity)
        {
            tasks = new List<TaskItem>();
            _logActivity = logActivity;
            // In a real application, you might load/save tasks from a file here
        }

        public void AddTask(string title, string description, DateTime? reminderDate = null)
        {
            TaskItem newTask = new TaskItem(title, description, reminderDate);
            tasks.Add(newTask);
            string reminderLog = reminderDate.HasValue ? $" with reminder for {reminderDate.Value:yyyy-MM-dd}" : "";
            _logActivity($"Task added: '{title}'{reminderLog}.");
        }

        public List<TaskItem> GetAllTasks()
        {
            return new List<TaskItem>(tasks); // Return a copy to prevent external modification
        }

        public bool MarkTaskAsCompleted(string title)
        {
            TaskItem task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (task != null)
            {
                task.IsCompleted = true;
                _logActivity($"Task marked completed: '{title}'.");
                return true;
            }
            return false;
        }

        public bool DeleteTask(string title)
        {
            TaskItem taskToRemove = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (taskToRemove != null)
            {
                tasks.Remove(taskToRemove);
                _logActivity($"Task deleted: '{title}'.");
                return true;
            }
            return false;
        }

        public List<TaskItem> GetOverdueTasks()
        {
            // Only consider tasks with reminders that are not completed and are past due
            return tasks.Where(t => !t.IsCompleted && t.ReminderDate.HasValue && t.ReminderDate.Value <= DateTime.Now).ToList();
        }
    }

    // ---
    // QuizItem Class
    // ---
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; } // Added for feedback
        public bool IsMultipleChoice => Options != null && Options.Any();

        public QuizQuestion(string question, string correctAnswer, string explanation, List<string> options = null)
        {
            Question = question;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
            Options = options;
        }
    }

    // ---
    // CybersecurityQuizGame Class
    // ---
    public class CybersecurityQuizGame
    {
        private List<QuizQuestion> allQuestions;
        private List<QuizQuestion> currentQuizQuestions; // Questions for the current game
        private Random random; // Only one declaration needed

        public int Score { get; private set; }
        public int TotalQuestionsAsked { get; private set; }

        public CybersecurityQuizGame()
        {
            random = new Random(); // Initialize the Random object here

            allQuestions = new List<QuizQuestion>
            {
                new QuizQuestion(
                    "What is phishing?",
                    "An attempt to trick you into revealing personal information",
                    "Phishing is a cybercrime where attackers try to deceive you into giving up sensitive information, often through fake emails or websites.",
                    new List<string> { "A type of malware", "A fishing technique", "An attempt to trick you into revealing personal information", "A software update" }
                ),
                new QuizQuestion(
                    "True or False: It's safe to use the same password for all your online accounts.",
                    "False",
                    "It's false! Using unique, strong passwords for each account protects you if one account is compromised. A password manager can help you manage them.",
                    new List<string> { "True", "False" }
                ),
                new QuizQuestion(
                    "Which of the following is a characteristic of a strong password?",
                    "A mix of uppercase and lowercase letters, numbers, and symbols",
                    "A strong password combines uppercase and lowercase letters, numbers, and symbols. It should also be long and difficult to guess.",
                    new List<string> { "Easy to remember", "Your birthdate", "A mix of uppercase and lowercase letters, numbers, and symbols", "A common word" }
                ),
                new QuizQuestion(
                    "What does 2FA stand for?",
                    "Two-Factor Authentication",
                    "2FA adds an extra layer of security to your accounts. Even if someone has your password, they'll need a second factor (like a code from your phone) to log in.",
                    new List<string> { "Two-Factor Authorization", "Two-Factor Authentication", "Two-File Access", "Total Financial Audit" }
                ),
                new QuizQuestion(
                    "What is malware?",
                    "Malicious software designed to harm or exploit a computer system",
                    "Malware is a broad term for any software intentionally designed to cause damage to a computer, server, client, or computer network.",
                    new List<string> { "A type of computer hardware", "Malicious software designed to harm or exploit a computer system", "A programming language", "An operating system" }
                ),
                new QuizQuestion(
                    "True or False: Clicking on suspicious links in emails is a safe practice if you know the sender.",
                    "False",
                    "Even if you know the sender, their account could be compromised. Always be cautious with links and attachments, especially if they seem unusual.",
                    new List<string> { "True", "False" }
                ),
                new QuizQuestion(
                    "What is the purpose of antivirus software?",
                    "To detect and remove malicious software",
                    "Antivirus software scans, detects, and removes viruses and other malicious software from your computer to protect your data.",
                    new List<string> { "To speed up your internet", "To organize your files", "To detect and remove malicious software", "To create documents" }
                ),
                new QuizQuestion(
                    "Which of these is a common social engineering tactic?",
                    "Pretexting",
                    "Pretexting involves creating a fabricated scenario to trick someone into divulging information or performing an action.",
                    new List<string> { "Encryption", "Firewall", "Pretexting", "Data backup" }
                ),
                new QuizQuestion(
                    "True or False: HTTPS in a website address means the connection is secure.",
                    "True",
                    "HTTPS encrypts the communication between your browser and the website, making it much harder for attackers to intercept your data.",
                    new List<string> { "True", "False" }
                ),
                new QuizQuestion(
                    "What should you do if you receive a suspicious email asking for your password?",
                    "Report it as phishing and delete it",
                    "Never give out your password via email. Report suspicious emails to your email provider or IT department, then delete them.",
                    new List<string> { "Reply with your password", "Delete the email", "Report it as phishing and delete it", "Ignore it" }
                ),
                 new QuizQuestion(
                    "What is a firewall?",
                    "A network security system that monitors and controls incoming and outgoing network traffic",
                    "A firewall acts as a barrier, preventing unauthorized access to or from a private network. It's a crucial part of network security.",
                    new List<string> { "A type of internet browser", "A tool for creating websites", "A network security system that monitors and controls incoming and outgoing network traffic", "A program to clean your hard drive" }
                ),
                new QuizQuestion(
                    "True or False: Public Wi-Fi networks are generally safe for sensitive transactions like online banking.",
                    "False",
                    "Public Wi-Fi networks are often unsecured, making it easy for attackers to intercept your data. Avoid sensitive transactions on public Wi-Fi or use a VPN.",
                    new List<string> { "True", "False" }
                ),
                new QuizQuestion(
                    "What is ransomware?",
                    "Malware that encrypts your files and demands payment to restore them",
                    "Ransomware is a type of malware that locks down your computer or encrypts your files and then demands a ransom (usually cryptocurrency) for their release.",
                    new List<string> { "A type of antivirus software", "A game that teaches cybersecurity", "Malware that encrypts your files and demands payment to restore them", "A backup solution" }
                )
            };
        }

        public void StartNewQuiz()
        {
            Score = 0;
            TotalQuestionsAsked = 0;
            currentQuizQuestions = allQuestions.OrderBy(x => random.Next()).Take(5).ToList(); // Take 5 random questions for a game
        }

        public QuizQuestion GetNextQuestion()
        {
            if (currentQuizQuestions != null && currentQuizQuestions.Any())
            {
                TotalQuestionsAsked++;
                QuizQuestion question = currentQuizQuestions[0];
                currentQuizQuestions.RemoveAt(0); // Remove the question once it's asked
                return question;
            }
            return null; // No more questions
        }

        public bool CheckAnswer(QuizQuestion question, string userAnswer)
        {
            bool isCorrect;
            if (question.IsMultipleChoice)
            {
                // Try to match by option letter (A, B, C)
                string normalizedUserAnswer = userAnswer.Trim().ToUpper();
                if (normalizedUserAnswer.Length == 1 && char.IsLetter(normalizedUserAnswer[0]))
                {
                    int index = normalizedUserAnswer[0] - 'A';
                    if (index >= 0 && index < question.Options.Count)
                    {
                        isCorrect = question.CorrectAnswer.Equals(question.Options[index], StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                else // If not a letter, try matching by full answer
                {
                    isCorrect = question.CorrectAnswer.Equals(userAnswer, StringComparison.OrdinalIgnoreCase);
                }
            }
            else // True/False
            {
                isCorrect = question.CorrectAnswer.Equals(userAnswer, StringComparison.OrdinalIgnoreCase);
            }

            if (isCorrect)
            {
                Score++;
            }
            return isCorrect;
        }
    }


    // ---
    // EnhancedResponseSystem Class
    // ---
    public class EnhancedResponseSystem
    {
        private Dictionary<string, List<string>> keywordResponses;
        private Dictionary<string, List<string>> topicResponses;
        private Dictionary<string, string> responses; // Keep the original responses
        public string currentTopic = null; // Changed to public to allow MainWindow to modify

        // Sentiment-related data
        private Dictionary<string, SentimentResponse> sentimentMap;

        // Instance of the task manager (used for chat-based task commands)
        private CybersecurityTaskManager _taskManager;
        private Action<string> _logActivity; // Delegate to log activities
        private Random random;

 

        public EnhancedResponseSystem(CybersecurityTaskManager taskManager, Action<string> logActivity)
        {
            _taskManager = taskManager; // Inject the task manager
            _logActivity = logActivity; // Inject the logging action
            random = new Random(); // Initialize the Random object here

            keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "password", new List<string> { "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.", "A strong password typically includes a mix of uppercase and lowercase letters, numbers, and symbols.", "Consider using a password manager to generate and store complex passwords securely." } },
                { "scam", new List<string> { "Be cautious of online scams. Never share personal or financial information with untrusted sources.", "Look out for red flags like urgent requests, poor grammar, or demands for immediate payment.", "If something seems too good to be true, it probably is a scam." } },
                { "privacy", new List<string> { "Protecting your privacy online involves being mindful of the information you share and with whom.", "Review the privacy settings of your social media and online accounts.", "Be wary of websites that ask for excessive personal information." } }
            };

            topicResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing tips", new List<string> {
                    "Be wary of emails from unknown senders, especially if they ask for personal information.",
                    "Never click on links or download attachments from suspicious emails.",
                    "Verify the sender's identity through official channels if you are unsure about an email's legitimacy.",
                    "Pay attention to the email's language and grammar; phishing attempts often contain errors.",
                    "Enable two-factor authentication on your email account for added security."
                }},
                { "malware advice", new List<string> {
                    "Install and keep your antivirus software up to date.",
                    "Be cautious when downloading files from the internet, especially from untrusted sources.",
                    "Avoid clicking on suspicious links or opening unexpected attachments.",
                    "Regularly scan your system for malware.",
                    "Keep your operating system and other software updated to patch security vulnerabilities."
                }},
                { "safe browse habits", new List<string> {
                    "Ensure that websites you visit, especially when entering sensitive information, use HTTPS.",
                    "Be careful about the permissions you grant to websites.",
                    "Consider using browser extensions that enhance security and privacy.",
                    "Keep your web browser updated to the latest version.",
                    "Avoid clicking on suspicious ads or pop-ups."
                }}
            };

            responses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "how are you", "As a large language model, I'm functioning well!" },
                { "what's your purpose", "My purpose is to assist you with information and tasks." },
                { "what can i ask you about", "You can ask me about a wide range of topics, including security topics." },
                { "password safety", "Use strong, unique passwords. Enable 2FA." },
                { "phishing", "Be wary of suspicious emails. Verify senders." },
                { "safe browse", "Keep software updated. Use antivirus." },
                { "tell me a joke", "Why don't scientists trust atoms? Because they make up everything!" },
                { "hello", "Hello there!" },
                { "hi", "Hi!" },
                { "goodbye", "Goodbye!" },
                { "bye", "Bye!" },
                { "thank you", "You're welcome!" },
                { "thanks", "You're welcome!" },
                { "what is the weather", "I do not have access to real time weather information." },
                { "what time is it", "I do not have access to real time information like the current time." },
                { "strong password", "A strong password should be long and complex." },
                { "two factor authentication", "2FA adds an extra layer of security." },
                { "https", "HTTPS indicates a secure connection." },
                { "ad blocker", "Ad blockers improve browse speed and security." },
                { "malware", "Malware can harm your system. Use antivirus." },
                { "start quiz", "Great! Let's start the cybersecurity quiz! I'll ask you a question. Type 'End Quiz' anytime to stop." }
                // "what have you done for me" removed as it's handled in MainWindow now
            };

            sentimentMap = new Dictionary<string, SentimentResponse>(StringComparer.OrdinalIgnoreCase)
            {
                { "worried", new SentimentResponse(Sentiment.Negative, "It's completely understandable to feel that way. Scammers can be very convincing. Let me share some tips to help you stay safe.", "Don't worry, I'm here to help you understand how to protect yourself.") },
                { "curious", new SentimentResponse(Sentiment.Neutral, "That's a great question! Let's explore that further.", "That's an interesting point. Here's some more information.") },
                { "frustrated", new SentimentResponse(Sentiment.Negative, "I understand it can be frustrating. Cybersecurity can seem complex, but we can break it down. What specifically is causing frustration?", "Take a deep breath. We'll work through this together. Tell me what's confusing you.") },
                { "unsure", new SentimentResponse(Sentiment.Negative, "It's alright to feel unsure. Let me clarify things for you.", "No problem at all. Let's go over it again.") },
                { "overwhelmed", new SentimentResponse(Sentiment.Negative, "I can see that this might seem like a lot. We can take it one step at a time.", "Let's focus on the most important aspects first.") }
            };
        }

        private enum Sentiment
        {
            Positive,
            Negative,
            Neutral
        }

        private class SentimentResponse
        {
            public Sentiment SentimentType { get; }
            public string SupportiveResponse { get; }
            public string EncouragingResponse { get; }

            public SentimentResponse(Sentiment sentimentType, string supportiveResponse, string encouragingResponse)
            {
                SentimentType = sentimentType;
                SupportiveResponse = supportiveResponse;
                EncouragingResponse = encouragingResponse;
            }
        }

        /// <summary>
        /// Handles task-related commands parsed directly from chat input using NLP simulation.
        /// </summary>
        /// <param name="lowerQuestion">The user's question in lowercase.</param>
        /// <returns>A response string if a task command was processed, otherwise null.</returns>
        public string HandleTaskCommand(string lowerQuestion)
        {
            // Regex to detect "add task", "set reminder", "remind me to", "create a task"
            // FIX: Added closing parenthesis to the last group in the regex pattern
            Match addTaskMatch = Regex.Match(lowerQuestion, @"(add|create)\s+(a\s+)?task\s+to\s+(.+)|(set|add)\s+(a\s+)?reminder\s+to\s+(.+)|remind\s+me\s+to\s+(.+)", RegexOptions.IgnoreCase);

            if (addTaskMatch.Success)
            {
                string fullCommand = addTaskMatch.Groups[0].Value;
                string taskContent = "";

                // Extract the task content based on which group matched
                if (!string.IsNullOrEmpty(addTaskMatch.Groups[3].Value))
                    taskContent = addTaskMatch.Groups[3].Value; // "add task to [content]"
                else if (!string.IsNullOrEmpty(addTaskMatch.Groups[6].Value))
                    taskContent = addTaskMatch.Groups[6].Value; // "set reminder to [content]"
                else if (!string.IsNullOrEmpty(addTaskMatch.Groups[7].Value))
                    taskContent = addTaskMatch.Groups[7].Value; // "remind me to [content]"

                if (!string.IsNullOrWhiteSpace(taskContent))
                {
                    string title = taskContent;
                    string description = taskContent; // Default description

                    DateTime? reminderDate = null;
                    Match dateMatch = Regex.Match(lowerQuestion, @"(tomorrow|in\s+(\d+)\s+days?|on\s+(\d{4}-\d{2}-\d{2}))");
                    if (dateMatch.Success)
                    {
                        if (dateMatch.Groups[1].Value == "tomorrow")
                        {
                            reminderDate = DateTime.Now.AddDays(1);
                        }
                        else if (dateMatch.Groups[2].Success && int.TryParse(dateMatch.Groups[2].Value, out int days))
                        {
                            reminderDate = DateTime.Now.AddDays(days);
                        }
                        else if (dateMatch.Groups[3].Success && DateTime.TryParse(dateMatch.Groups[3].Value, out DateTime parsedDate))
                        {
                            reminderDate = parsedDate;
                        }

                        // Remove the date part from the task content to make the title cleaner
                        title = Regex.Replace(taskContent, @"\s+(tomorrow|in\s+\d+\s+days?|on\s+\d{4}-\d{2}-\d{2})", "", RegexOptions.IgnoreCase).Trim();
                        if (string.IsNullOrWhiteSpace(title)) title = taskContent; // Fallback if removing date makes title empty
                    }

                    _taskManager.AddTask(title, description, reminderDate); // Use injected task manager
                    string reminderConfirm = reminderDate.HasValue ? $" for '{title}' on {reminderDate.Value:yyyy-MM-dd}." : $" for '{title}'. Would you like to set a reminder for this task?";
                    _logActivity($"NLP interpreted task added: '{title}'{(reminderDate.HasValue ? " with reminder" : "")}."); // Log NLP action
                    return $"Task added: '{title}'. {reminderConfirm}";
                }
            }

            // Explicit commands for task management
            if (lowerQuestion.Contains("show tasks") || lowerQuestion.Contains("view tasks") || lowerQuestion == "list my tasks")
            {
                // TaskManager's GetAllTasks() does not log, so log here
                _logActivity("User requested to view tasks.");
                List<TaskItem> allTasks = _taskManager.GetAllTasks();
                if (allTasks.Any())
                {
                    StringBuilder sb = new StringBuilder("Here are your cybersecurity tasks:\n");
                    foreach (var task in allTasks)
                    {
                        sb.AppendLine($"- {task}");
                    }
                    return sb.ToString();
                }
                return "You currently have no cybersecurity tasks.";
            }
            else if (lowerQuestion.Contains("mark task completed "))
            {
                string taskTitle = lowerQuestion.Replace("mark task completed ", "").Trim();
                if (!string.IsNullOrWhiteSpace(taskTitle))
                {
                    if (_taskManager.MarkTaskAsCompleted(taskTitle)) // TaskManager logs
                    {
                        return $"Task '{taskTitle}' marked as completed.";
                    }
                    return $"Could not find task '{taskTitle}'.";
                }
                return "Please specify the task to mark as completed: 'Mark task completed [Task Title]'";
            }
            else if (lowerQuestion.Contains("delete task "))
            {
                string taskTitle = lowerQuestion.Replace("delete task ", "").Trim();
                if (!string.IsNullOrWhiteSpace(taskTitle))
                {
                    if (_taskManager.DeleteTask(taskTitle)) // TaskManager logs
                    {
                        return $"Task '{taskTitle}' deleted.";
                    }
                    return $"Could not find task '{taskTitle}'.";
                }
                return "Please specify the task to delete: 'Delete task [Task Title]'";
            }

            return null; // Not a task command
        }

        public string GetResponse(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return "I didn't quite understand that. Could you rephrase?";
            }

            string lowerQuestion = question.ToLower();
            // FIX: Declare 'response' once at the start of the method to avoid re-declaration errors
            string response = null;

            // Handle "What have you done for me" / "Show activity log" in MainWindow
            // as MainWindow holds the activityLog.

            // 1. Try to handle task commands using NLP simulation
            string taskResponse = HandleTaskCommand(lowerQuestion);
            if (taskResponse != null)
            {
                currentTopic = null; // Reset topic after a task command
                return taskResponse;
            }

            // 2. Check for sentiment keywords
            foreach (var sentimentPair in sentimentMap)
            {
                if (lowerQuestion.Contains(sentimentPair.Key))
                {
                    currentTopic = null; // Reset topic as the focus is on sentiment
                    _logActivity($"User expressed '{sentimentPair.Key}' sentiment. Provided supportive response."); // Log sentiment
                    return sentimentPair.Value.SupportiveResponse;
                }
            }

            // 3. Check for specific topic queries with multiple responses
            foreach (var topicPair in topicResponses)
            {
                if (topicPair.Key.Split(' ').Any(keyword => lowerQuestion.Contains(keyword)))
                {
                    currentTopic = topicPair.Key;
                    // FIX: Assign to the already declared 'response' variable
                    response = topicPair.Value[random.Next(topicPair.Value.Count)];
                    _logActivity($"Responded to topic '{topicPair.Key}'."); // Log topic response
                    return response;
                }
            }

            // 4. Check for keyword matches with multiple responses
            foreach (var keywordPair in keywordResponses)
            {
                if (keywordPair.Key.Split(' ').Any(keyword => lowerQuestion.Contains(keyword)))
                {
                    currentTopic = keywordPair.Key;
                    // FIX: Assign to the already declared 'response' variable
                    response = keywordPair.Value[random.Next(keywordPair.Value.Count)];
                    _logActivity($"Responded to keyword '{keywordPair.Key}'."); // Log keyword response
                    return response;
                }
            }

            // 5. Fallback to the original response system for exact matches
            // FIX: 'out response' is now referencing the 'response' declared at the top
            if (responses.TryGetValue(lowerQuestion, out response))
            {
                currentTopic = lowerQuestion;
                _logActivity($"Responded to exact match '{lowerQuestion}'."); // Log exact match
                return response;
            }

            currentTopic = null; // Reset current topic if no match
            _logActivity($"Did not understand user input: '{question}'."); // Log unknown input
            return "I didn't quite understand that. Could you rephrase?";
        }
    }

    // ---
    // MainWindow Class (The primary WPF window)
    // ---
    public partial class MainWindow : Window
    {
        private EnhancedResponseSystem responseSystem;
        private CybersecurityTaskManager taskManager; // Reference to the task manager
        private CybersecurityQuizGame quizGame; // Instance of the quiz game
        private QuizQuestion currentQuizQuestion; // To hold the current question
        private List<ActivityLogEntry> activityLog; // Activity Log storage
        private const int MaxLogEntries = 10; // Limit for activity log

        private const string InitialUserInputText = "Type your question here...";
        private DispatcherTimer reminderTimer; // WPF specific timer

        // Define the path to your ASCII art file
        // IMPORTANT: Replace "YourUserName" with your actual Windows username

        private static readonly string AsciiArtFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "ascii-art.txt");

        public MainWindow()
        {
            InitializeComponent();
            activityLog = new List<ActivityLogEntry>(); // Initialize the activity log
            LogActivity("Chatbot started."); // Log chatbot start

            taskManager = new CybersecurityTaskManager(LogActivity); // Pass LogActivity delegate
            responseSystem = new EnhancedResponseSystem(taskManager, LogActivity); // Pass task manager and LogActivity delegate
            quizGame = new CybersecurityQuizGame(); // Initialize the quiz game (now correctly initializes its 'random' field)

            // Set the DisplayDateStart for the DatePicker
            ReminderDatePicker.DisplayDateStart = DateTime.Now;


            // Initial chatbot message
            DisplayChatbotMessage("Hello! I'm your Enhanced Cybersecurity Chatbot and Task Manager. Ask me anything about cybersecurity or manage your tasks!");
            DisplayChatbotMessage("You can also add tasks via chat like: 'Add a task to review privacy settings' or 'Remind me to update password tomorrow'.");
            DisplayChatbotMessage("Or use the dedicated Task Manager section to 'Add Task', 'Show tasks', 'Mark task completed [Title]', 'Delete task [Title]'.");
            DisplayChatbotMessage("Feeling like a challenge? Type 'Start Quiz' to test your cybersecurity knowledge!");
            DisplayChatbotMessage("Want to see what I've been up to? Type 'Show activity log' or 'What have you done for me?'.");

            UserInputTextBox.Text = InitialUserInputText; // Set initial placeholder text
            UserInputTextBox.Foreground = Brushes.Gray; // Set placeholder color

            RefreshTaskListDisplay(); // Display any initial tasks
            StartReminderTimer(); // Start the reminder checking (This call is now valid within the class scope)
        }

        /// <summary>
        /// Displays a message from the chatbot in the chat output area.
        /// </summary>
        private void DisplayChatbotMessage(string message)
        {
            TextBlock botTextBlock = new TextBlock
            {
                Text = "Bot: " + message,
                Foreground = Brushes.Green,
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap // Enable text wrapping
            };
            ChatOutputStackPanel.Children.Add(botTextBlock);
            // Scroll to the bottom
            if (ChatOutputScrollViewer != null)
            {
                ChatOutputScrollViewer.ScrollToEnd();
            }
        }

        /// <summary>
        /// Displays a message from the user in the chat output area.
        /// </summary>
        private void DisplayUserMessage(string message)
        {
            TextBlock userTextBlock = new TextBlock
            {
                Text = "You: " + message,
                Foreground = Brushes.Yellow,
                HorizontalAlignment = HorizontalAlignment.Right, // Align user messages to the right
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap // Enable text wrapping
            };
            ChatOutputStackPanel.Children.Add(userTextBlock);
            // Scroll to the bottom
            if (ChatOutputScrollViewer != null)
            {
                ChatOutputScrollViewer.ScrollToEnd();
            }
        }

        /// <summary>
        /// Logs an activity description with a timestamp to the internal activity log.
        /// Keeps the log length limited to MaxLogEntries.
        /// </summary>
        /// <param name="description">The description of the activity.</param>
        private void LogActivity(string description)
        {
            if (activityLog.Count >= MaxLogEntries)
            {
                activityLog.RemoveAt(0); // Remove the oldest entry
            }
            activityLog.Add(new ActivityLogEntry(description));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        /// <summary>
        /// Processes the user's input, routing it to quiz, task, or general response logic.
        /// </summary>
        private void ProcessUserInput()
        {
            string userInput = UserInputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput) || userInput == InitialUserInputText)
            {
                DisplayChatbotMessage("Please type something to get a response.");
                return;
            }

            DisplayUserMessage(userInput);
            string lowerUserInput = userInput.ToLower();
            LogActivity($"User input: '{userInput}'."); // Log user input

            // Quiz game logic takes precedence if a quiz is active or if quiz commands are given
            if (currentQuizQuestion != null)
            {
                HandleQuizAnswer(userInput);
            }
            else if (lowerUserInput == "start quiz")
            {
                StartQuiz();
            }
            else if (lowerUserInput == "next question")
            {
                GetNextQuizQuestion();
            }
            else if (lowerUserInput == "end quiz")
            {
                EndQuiz();
            }
            // Handle activity log requests
            else if (lowerUserInput == "show activity log" || lowerUserInput == "what have you done for me?")
            {
                ShowActivityLog();
            }
            else
            {
                // General chatbot response, now also handles NLP-simulated task commands
                string response = responseSystem.GetResponse(userInput);
                DisplayChatbotMessage(response);

                // If a task command was processed via chat, refresh the task list
                // Check for explicit task added/updated/deleted messages from HandleTaskCommand
                if (response.Contains("Task added:") || response.Contains("Task '") && (response.Contains("marked as completed") || response.Contains("deleted")))
                {
                    RefreshTaskListDisplay();
                }
            }

            UserInputTextBox.Text = string.Empty; // Clear the input box
            UserInputTextBox.Foreground = Brushes.Gray; // Reset color after clearing
        }

        // --- Quiz Game Methods ---
        private void StartQuiz()
        {
            quizGame.StartNewQuiz(); // Initialize score and questions
            LogActivity("Quiz started."); // Log quiz start
            DisplayChatbotMessage("Welcome to the Cybersecurity Quiz! Let's test your knowledge.");
            GetNextQuizQuestion();
        }

        private void GetNextQuizQuestion()
        {
            currentQuizQuestion = quizGame.GetNextQuestion();
            if (currentQuizQuestion != null)
            {
                LogActivity($"Presented quiz question {quizGame.TotalQuestionsAsked}: '{currentQuizQuestion.Question}'."); // Log question asked
                StringBuilder questionText = new StringBuilder($"Question {quizGame.TotalQuestionsAsked}. {currentQuizQuestion.Question}");
                if (currentQuizQuestion.IsMultipleChoice)
                {
                    questionText.AppendLine("\nOptions:");
                    char optionChar = 'A';
                    foreach (var option in currentQuizQuestion.Options)
                    {
                        questionText.AppendLine($"{optionChar++}. {option}");
                    }
                    questionText.AppendLine("Type the letter (A, B, C...) or the full answer.");
                }
                else
                {
                    questionText.AppendLine("\nType 'True' or 'False'.");
                }
                DisplayChatbotMessage(questionText.ToString());
            }
            else
            {
                EndQuiz(); // No more questions
            }
        }

        private void HandleQuizAnswer(string userAnswer)
        {
            if (currentQuizQuestion == null)
            {
                DisplayChatbotMessage("There's no active quiz question. Type 'Start Quiz' to begin.");
                LogActivity("User attempted to answer quiz without active question.");
                return;
            }

            bool isCorrect = quizGame.CheckAnswer(currentQuizQuestion, userAnswer);

            if (isCorrect)
            {
                DisplayChatbotMessage($"**Correct!** {currentQuizQuestion.Explanation}");
                LogActivity($"Quiz answer correct for '{currentQuizQuestion.Question}'. Score: {quizGame.Score}/{quizGame.TotalQuestionsAsked}.");
            }
            else
            {
                // Find the option letter if it was a multiple choice question and user answered with a letter
                string correctOptionLetter = "";
                if (currentQuizQuestion.IsMultipleChoice)
                {
                    int correctIndex = currentQuizQuestion.Options.FindIndex(opt => opt.Equals(currentQuizQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
                    if (correctIndex != -1)
                    {
                        correctOptionLetter = $" ({(char)('A' + correctIndex)})";
                    }
                }
                DisplayChatbotMessage($"**Incorrect.** The correct answer was: **{currentQuizQuestion.CorrectAnswer}**{correctOptionLetter}. {currentQuizQuestion.Explanation}");
                LogActivity($"Quiz answer incorrect for '{currentQuizQuestion.Question}'. Correct: '{currentQuizQuestion.CorrectAnswer}'. Score: {quizGame.Score}/{quizGame.TotalQuestionsAsked}.");
            }

            // After feedback, check if more questions are available or if quiz should end
            if (quizGame.TotalQuestionsAsked < 5) // Assuming a fixed number of questions per game (e.g., 5)
            {
                DisplayChatbotMessage("Type 'Next Question' for the next one, or 'End Quiz' to finish.");
                currentQuizQuestion = null; // Reset to allow for "Next Question" command
            }
            else
            {
                EndQuiz(); // All questions for this round have been asked
            }
        }

        private void EndQuiz()
        {
            string feedback;
            double scorePercentage = (double)quizGame.Score / quizGame.TotalQuestionsAsked * 100;

            if (quizGame.TotalQuestionsAsked == 0)
            {
                feedback = "You ended the quiz before answering any questions. No score to display.";
                LogActivity("Quiz ended by user, no questions answered.");
            }
            else
            {
                LogActivity($"Quiz completed. Final score: {quizGame.Score} out of {quizGame.TotalQuestionsAsked}."); // Log quiz completion
                if (scorePercentage == 100)
                {
                    feedback = "🎉 **Perfect score!** You're a true cybersecurity pro! Keep up the excellent work!";
                }
                else if (scorePercentage >= 70)
                {
                    feedback = "👍 **Great job!** You have a solid understanding of cybersecurity concepts. Keep learning to stay sharp!";
                }
                else if (scorePercentage >= 40)
                {
                    feedback = "Keep learning! You're on your way to becoming more cybersecurity aware. Reviewing the tips in our chat history can help.";
                }
                else
                {
                    feedback = "That was a good start! Cybersecurity can be tricky, but continuous learning is key. Don't worry, you can always try again and check out the various tips I provide!";
                }
            }


            DisplayChatbotMessage($"Quiz ended! Your score: **{quizGame.Score} out of {quizGame.TotalQuestionsAsked}** questions correct. {feedback}");
            currentQuizQuestion = null; // Ensure quiz state is reset
            // quizGame.StartNewQuiz(); // This is implicitly handled when 'Start Quiz' is next called
        }

        /// <summary>
        /// Displays the recent activity log to the user.
        /// </summary>
        private void ShowActivityLog()
        {
            if (activityLog.Any())
            {
                StringBuilder sb = new StringBuilder("Here's a summary of recent actions:\n");
                // Display up to MaxLogEntries, most recent first
                var recentActivities = activityLog.AsEnumerable().Reverse().Take(MaxLogEntries);

                foreach (var entry in recentActivities)
                {
                    // entry.ToString() already includes timestamp
                    sb.AppendLine($"- {entry.ToString()}");
                }
                DisplayChatbotMessage(sb.ToString());
            }
            else
            {
                DisplayChatbotMessage("No recent activity to display yet.");
            }
            LogActivity("User requested activity log."); // Log viewing the log
        }

        // --- Task Manager Methods ---
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            DateTime? reminderDate = ReminderDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please enter a title and description for the task.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            taskManager.AddTask(title, description, reminderDate); // TaskManager will log this
            DisplayChatbotMessage($"Task '{title}' added successfully via Task Manager interface!");

            // Clear task input fields
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            ReminderDatePicker.SelectedDate = null;

            RefreshTaskListDisplay();
        }

        private void ViewManageTasksButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTaskListDisplay();
            DisplayChatbotMessage("Here are your current tasks.");
            LogActivity("User requested to view tasks via GUI."); // Log GUI action
        }

        private void RefreshTaskListDisplay()
        {
            TaskListDisplay.Items.Clear();
            List<TaskItem> allTasks = taskManager.GetAllTasks();
            if (allTasks.Any())
            {
                // Sort tasks: pending first, then completed. Within each, by reminder date or added date.
                var sortedTasks = allTasks.OrderBy(t => t.IsCompleted) // False (pending) comes before true (completed)
                                          .ThenBy(t => t.ReminderDate.HasValue ? t.ReminderDate.Value : DateTime.MaxValue) // Reminder tasks earlier
                                          .ThenBy(t => t.DateAdded) // Then by date added
                                          .ToList();

                foreach (var task in sortedTasks)
                {
                    // Create a StackPanel for each task to hold its details and buttons
                    StackPanel taskPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                    // Task details TextBlock
                    TextBlock taskTextBlock = new TextBlock
                    {
                        Text = task.ToString(),
                        // Using a fixed width or Grid.ColumnDefinitions for better layout
                        Width = 350,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null, // Strikethrough if completed
                        Foreground = task.IsCompleted ? Brushes.Gray : Brushes.White // Dim completed tasks
                    };

                    taskPanel.Children.Add(taskTextBlock);

                    // Add "Complete" button if the task is not completed
                    if (!task.IsCompleted)
                    {
                        Button completeButton = new Button
                        {
                            Content = "Complete",
                            Tag = task.Title, // Use Tag to store the task title for event handling
                            Margin = new Thickness(5, 0, 0, 0),
                            Style = (Style)FindResource("TaskActionButtonStyle") // Apply a style if defined in XAML
                        };
                        completeButton.Click += (s, ev) =>
                        {
                            if (taskManager.MarkTaskAsCompleted((string)((Button)s).Tag)) // TaskManager will log this
                            {
                                DisplayChatbotMessage($"Task '{(string)((Button)s).Tag}' marked as completed.");
                                RefreshTaskListDisplay();
                            }
                        };
                        taskPanel.Children.Add(completeButton);
                    }

                    // Add "Delete" button
                    Button deleteButton = new Button
                    {
                        Content = "Delete",
                        Tag = task.Title, // Use Tag to store the task title
                        Margin = new Thickness(5, 0, 0, 0),
                        Style = (Style)FindResource("TaskActionButtonStyle") // Apply a style if defined in XAML
                    };
                    deleteButton.Click += (s, ev) =>
                    {
                        if (MessageBox.Show($"Are you sure you want to delete task '{(string)((Button)s).Tag}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            if (taskManager.DeleteTask((string)((Button)s).Tag)) // TaskManager will log this
                            {
                                DisplayChatbotMessage($"Task '{(string)((Button)s).Tag}' deleted.");
                                RefreshTaskListDisplay();
                            }
                        }
                    };
                    taskPanel.Children.Add(deleteButton);

                    TaskListDisplay.Items.Add(taskPanel);
                }
            }
            else
            {
                TaskListDisplay.Items.Add(new TextBlock { Text = "No tasks currently added.", Foreground = Brushes.White, FontStyle = FontStyles.Italic });
            }
        }

        // Placeholder text logic for the user input textbox
        private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserInputTextBox.Text == InitialUserInputText)
            {
                UserInputTextBox.Text = string.Empty;
                UserInputTextBox.Foreground = Brushes.White; // Change text color when user types
            }
        }

        private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInputTextBox.Text))
            {
                UserInputTextBox.Text = InitialUserInputText;
                UserInputTextBox.Foreground = Brushes.Gray; // Restore placeholder color
            }
        }

        // Reminder System using DispatcherTimer
        private void StartReminderTimer()
        {
            reminderTimer = new DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(30); // Check every 30 seconds for reminders
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
            LogActivity("Reminder timer started."); // Log timer start
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            List<TaskItem> overdueTasks = taskManager.GetOverdueTasks();
            foreach (var task in overdueTasks)
            {
                // Display reminder to the user in the chatbot's output area
                DisplayChatbotMessage($"🔔 REMINDER: It's time for your task: '{task.Title}'! \"{task.Description}\"");
                LogActivity($"Reminder notification for task: '{task.Title}'."); // Log reminder notification
                // To prevent repeated notifications for the same task, you might consider:
                // 1. Marking the reminder as "dismissed" (e.g., adding a property to TaskItem).
                // 2. Only showing the reminder once per session for a given task.
                // For this example, it will keep showing until the task is completed or deleted.
            }
        }
    }
}
﻿using SmartData.Lib.Interfaces;

using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SmartData.Lib.Services
{
    public class LoggerService : ILoggerService, INotifyPropertyChanged
    {
        private string _latestLogMessage = string.Empty;
        public string LatestLogMessage
        {
            get => _latestLogMessage;
            set
            {
                _latestLogMessage = value;
                OnPropertyChanged(nameof(LatestLogMessage));
                CleanLogMessage(TimeSpan.FromSeconds(60));
            }
        }

        private Stopwatch _stopwatch = new Stopwatch();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Cleans the latest log message after a specified time span.
        /// </summary>
        /// <param name="timeSpan">The time span after which the latest log message should be cleaned.</param>
        /// <remarks>
        /// This method is invoked to clean the latest log message after a specified time span has elapsed.
        /// It starts by restarting the internal stopwatch to measure the time elapsed.
        /// The method then asynchronously delays for the specified time span using the <see cref="Task.Delay"/> method.
        /// After the delay, it sets the value of the latest log message to an empty string, effectively cleaning it.
        /// It raises the <see cref="PropertyChanged"/> event for the "LatestLogMessage" property to notify any subscribers of the change.
        /// Finally, it stops the stopwatch and the method execution completes.
        /// </remarks>
        private async void CleanLogMessage(TimeSpan timeSpan)
        {
            _stopwatch.Restart();
            await Task.Delay(timeSpan);
            _latestLogMessage = string.Empty;
            OnPropertyChanged(nameof(LatestLogMessage));
            _stopwatch.Stop();
        }

        /// <summary>
        /// Saves the details of an exception and its stack trace to a text file.
        /// </summary>
        /// <param name="exception">The exception to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method creates a text file in the "logs" folder to store the details of the specified exception.
        /// It constructs a string containing the exception details, including the date and time, source, message, help link, HResult, and inner exception details if present.
        /// The method appends the stack trace and target site information to the string.
        /// It also includes additional information from the exception's data dictionary, if available.
        /// Finally, it appends the constructed string to the text file.
        /// </remarks>
        public async Task SaveExceptionStackTrace(Exception exception)
        {
            string outputFolder = Path.Combine(Environment.CurrentDirectory, "logs");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            string filePath = Path.Combine(outputFolder, $"{DateTime.Now.ToString("error_yyyy-MM-dd_HH-mm-ss")}.txt");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Exception Details");
            stringBuilder.AppendLine("=================");
            stringBuilder.AppendLine($"Date and Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.AppendLine($"Source: {exception.Source}");
            stringBuilder.AppendLine($"Message: {exception.Message}");
            stringBuilder.AppendLine($"Help Link: {exception.HelpLink}");
            stringBuilder.AppendLine($"HResult: {exception.HResult}");
            stringBuilder.AppendLine();

            if (exception.InnerException != null)
            {
                stringBuilder.AppendLine("Inner Exception Details");
                stringBuilder.AppendLine("======================");
                stringBuilder.AppendLine($"Source: {exception.InnerException.Source}");
                stringBuilder.AppendLine($"Message: {exception.InnerException.Message}");
                stringBuilder.AppendLine($"Help Link: {exception.InnerException.HelpLink}");
                stringBuilder.AppendLine($"HResult: {exception.InnerException.HResult}");
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine("Stack Trace");
            stringBuilder.AppendLine("============");
            stringBuilder.AppendLine(exception.StackTrace);
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("Target Site");
            stringBuilder.AppendLine("============");
            stringBuilder.AppendLine($"Declaring Type: {exception.TargetSite.DeclaringType}");
            stringBuilder.AppendLine($"Method Name: {exception.TargetSite.Name}");
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("Additional Information");
            stringBuilder.AppendLine("======================");
            foreach (var key in exception.Data.Keys)
            {
                stringBuilder.AppendLine($"{key}: {exception.Data[key]}");
            }

            await File.AppendAllTextAsync(filePath, stringBuilder.ToString());
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

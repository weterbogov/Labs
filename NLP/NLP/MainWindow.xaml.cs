﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NLP
{
    public partial class MainWindow : Window
    {
        DictionaryContext db = new DictionaryContext();
        public ObservableCollection<Word> WordDictionary { get; set; } = new ObservableCollection<Word>();

        public List<string> Files { get; set; } = new List<string>();

        public SortingType CurrentSortingType { get; set; } = SortingType.None;

        public MainWindow()
        {
            InitializeComponent();
            WordDictionary = new ObservableCollection<Word>(db.Words);
            DataContext = this;
        }

        #region Events

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                Files.Add(openFileDialog.FileName);
                using (FileStream fstream = File.OpenRead(openFileDialog.FileName))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string text = Encoding.UTF8.GetString(array);
                    int counter = 0;
                    IEnumerable<string> words = Regex.Matches(text, "(?<word>[a-zA-Z-—]+('(s|d|ve|ll))?)(<[a-zA-Z]+>)?").Cast<Match>().Select(x => x.Groups["word"].Value);
                    foreach (var word in words)
                    {
                        if (!String.IsNullOrWhiteSpace(word))
                        {
                            var currentWord = WordDictionary.FirstOrDefault(x => x.Name == word);
                            if (currentWord == null)
                            {
                                WordDictionary.Add(new Word(word, 1));
                            }
                            else
                            {
                                currentWord.Amount++;
                            }
                        }

                        counter++;
                        ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = counter * 100 / words.Count(), DispatcherPriority.Background);
                    }

                    ClearWordsDatabase();
                    db.Words.AddRange(WordDictionary.ToList());
                    db.SaveChanges();
                    StatusLine.Text = $"{openFileDialog.FileName} has been parsed.";
                }
            }
        }

        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            WordDictionary.Clear();
            ClearWordsDatabase();
            StatusLine.Text = "Table has been cleared.";
        }

        private void NameTableHeader_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSortingType == SortingType.NameAscending)
            {
                CurrentSortingType = SortingType.NameDescending;
                UpdateWordDictionary(WordDictionary.OrderByDescending(x => x.Name).ToList());
            }
            else
            {
                CurrentSortingType = SortingType.NameAscending;
                UpdateWordDictionary(WordDictionary.OrderBy(x => x.Name).ToList());
            }
        }

        private void AmountTableHeader_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSortingType == SortingType.AmountDescending)
            {
                CurrentSortingType = SortingType.AmountAscending;
                UpdateWordDictionary(WordDictionary.OrderBy(x => x.Amount).ToList());
            }
            else
            {
                CurrentSortingType = SortingType.AmountDescending;
                UpdateWordDictionary(WordDictionary.OrderByDescending(x => x.Amount).ToList());
            }
        }

        private void UpdateWord_Click(object sender, RoutedEventArgs e)
        {
            var modalWindow = new WordEditingModalWindow();
            if (modalWindow.ShowDialog() == true)
            {
                if (modalWindow.NewWordTextBox.Text != String.Empty && modalWindow.OldWordTextBox.Text != String.Empty)
                {
                    Files.ForEach(x => ReplaceWord(x, modalWindow.OldWordTextBox.Text, modalWindow.NewWordTextBox.Text));
                }
            }
        }

        private void AnalyzeText_Click(object sender, RoutedEventArgs e)
        {
            foreach (var word in WordDictionary)
            {
                Files.ForEach(x => ReplaceTaggedWord(x, word.Name, word.GetAnalyzedWord()));
            }
        }

        #endregion

        #region Supporting functions

        private void ClearWordsDatabase() => db.Database.ExecuteSqlCommand("DELETE FROM [Words]");

        private void UpdateWordDictionary(List<Word> list)
        {
            WordDictionary.Clear();
            foreach (var word in list)
            {
                WordDictionary.Add(word);
            }
        }

        private void ReplaceWord(string filename, string oldWord, string newWord)
        {
            string text;
            using (FileStream fstream = File.OpenRead(filename))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                text = Encoding.UTF8.GetString(array);
            }

            text = text.Replace(oldWord, newWord);
            File.WriteAllText(filename, text);
        }

        private void ReplaceTaggedWord(string filename, string oldWord, string newWord)
        {
            string text;
            using (FileStream fstream = File.OpenRead(filename))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                text = Encoding.UTF8.GetString(array);
            }

            text = text.Replace($" {oldWord} ", $" {newWord} ");
            text = text.Replace($" {oldWord}.", $" {newWord}.");
            text = text.Replace($" {oldWord},", $" {newWord},");
            text = text.Replace($" {oldWord};", $" {newWord};");
            text = text.Replace($" {oldWord}!", $" {newWord}!");
            text = text.Replace($" {oldWord}?", $" {newWord}?");
            text = text.Replace($" {oldWord}\n", $" {newWord}\n");
            text = text.Replace($"{oldWord} ", $"{newWord} ");
            text = text.Replace($"{oldWord}.", $"{newWord}.");
            text = text.Replace($"{oldWord},", $"{newWord},");
            text = text.Replace($"{oldWord};", $"{newWord};");
            text = text.Replace($"{oldWord}!", $"{newWord}!");
            text = text.Replace($"{oldWord}?", $"{newWord}?");
            text = text.Replace($"{oldWord}\n", $"{newWord}\n");
            File.WriteAllText(filename, text);
        }

        #endregion
    }

    public enum SortingType
    {
        NameAscending,
        NameDescending,
        AmountAscending,
        AmountDescending,
        None
    }
}

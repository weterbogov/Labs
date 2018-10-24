﻿using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const uint Phi = 0x9e3779b9;

        public List<int> MainSubKeys = new List<int>() { 437886378, 1255596225, 421721128, 1364094158, 356429358, 1877769357, 905174355, 1423700773 };

        public byte[] Bytes { get; set; }

        public string CurrentFileName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                CurrentFileName = openFileDialog.FileName;
                Bytes = File.ReadAllBytes(CurrentFileName);
                FileNameTB.Text = openFileDialog.SafeFileName;
            }
        }

        private void GenerateRSA_Click(object sender, RoutedEventArgs e)
        {
            string directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
            RsaGenerator.Generate();
            //var bytes = new byte()[];
            //File.WriteAllBytes(Path.Combine(directory, "MyRSA.txt"), bytes);
        }

        private void Encode_Click(object sender, RoutedEventArgs e)
        {
            var initialBits = new BitArray(Bytes);
            var bits = new BitArray(initialBits);
            for (int i = 0; i < 32; i++)
            {
                bits.Set(4 * i, initialBits[i]);
                bits.Set(4 * i + 1, initialBits[i + 32]);
                bits.Set(4 * i + 2, initialBits[i + 64]);
                bits.Set(4 * i + 3, initialBits[i + 96]);
            }
            var subKeys = GetSubKeys(MainSubKeys).ToList();
            for (int i = 0; i <= 32; i++)
            {
                bits = bits.Xor(subKeys[i]);
            }
            var finalBits = new BitArray(bits);
            for (int i = 0; i < 32; i++)
            {
                finalBits.Set(i, bits[4 * i]);
                finalBits.Set(i + 32, bits[4 * i + 1]);
                finalBits.Set(i + 64, bits[4 * i + 2]);
                finalBits.Set(i + 96, bits[4 * i + 3]);
            }
            byte[] bytes = new byte[16];
            finalBits.CopyTo(bytes, 0);
            File.WriteAllBytes(AddStringToFileName(CurrentFileName, $"_Encoded"), bytes);
        }

        private void Decode_Click(object sender, RoutedEventArgs e)
        {
            var initialBits = new BitArray(Bytes);
            var bits = new BitArray(initialBits);
            for (int i = 0; i < 32; i++)
            {
                bits.Set(4 * i, initialBits[i]);
                bits.Set(4 * i + 1, initialBits[i + 32]);
                bits.Set(4 * i + 2, initialBits[i + 64]);
                bits.Set(4 * i + 3, initialBits[i + 96]);
            }
            var subKeys = GetSubKeys(MainSubKeys).ToList();
            for (int i = 32; i >= 0; i--)
            {
                bits = bits.Xor(subKeys[i]);
            }
            var finalBits = new BitArray(bits);
            for (int i = 0; i < 32; i++)
            {
                finalBits.Set(i, bits[4 * i]);
                finalBits.Set(i + 32, bits[4 * i + 1]);
                finalBits.Set(i + 64, bits[4 * i + 2]);
                finalBits.Set(i + 96, bits[4 * i + 3]);
            }

            byte[] bytes = new byte[16];
            finalBits.CopyTo(bytes, 0);
            File.WriteAllBytes(AddStringToFileName(CurrentFileName, $"_Decoded"), bytes);
        }

        private IEnumerable<BitArray> GetSubKeys(List<int> mainSubKeys)
        {
            var subKeys = new List<int>();
            subKeys.AddRange(mainSubKeys);
            for (int i = 8; i < 140; i++)
            {
                subKeys.Add(subKeys[i-8] ^ subKeys[i - 5] ^ subKeys[i - 3] ^ subKeys[i - 1] ^ unchecked((int)Phi) ^ i);
            }
            for (int i = 0; i < 33; i++)
            {
                yield return new BitArray(new int[] { subKeys[4 * i + 8], subKeys[4 * i + 9], subKeys[4 * i + 10], subKeys[4 * i + 11] });
            }
        }

        private string AddStringToFileName(string fileName, string line) =>
            Path.Combine(Path.GetDirectoryName(fileName),
                Path.GetFileNameWithoutExtension(fileName) + line + Path.GetExtension(fileName));
    }
}

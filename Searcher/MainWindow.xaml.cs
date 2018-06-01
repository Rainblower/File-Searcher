using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace Searcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Vars
        Task task = null;
        bool isStopped;
        bool isCheck;
        bool isPaused;
        bool isWorked = false;
        int sec = 0;
        int count = 0;
        string open;
        private HashSet<string> context;
        List<string> list = new List<string>();
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            txtName.Text = Properties.Settings.Default.Name;
            txtOpen.Text = Properties.Settings.Default.Path;
            cb.IsChecked = Properties.Settings.Default.Check;

            if (txtOpen.Text != "")
            {
                btnSearch.IsEnabled = true;
                open = txtOpen.Text;
            }
        }

        #region Tasks
        async Task Searching()
        {
            task = Task.Factory.StartNew(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    list.AddRange(Directory.GetFiles(open, $"*{txtName.Text}*", SearchOption.AllDirectories));
                    foreach (var item in list.ToList())
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (!isStopped)
                            {
                                if (!isPaused)
                                {
                                    FileInfo fi = new FileInfo(item);
                                    if (isCheck)
                                    {
                                        var fileContext = File.ReadAllLines(fi.FullName);
                                        context = new HashSet<string>(fileContext);
                                        if (context.Contains(txtText.Text))
                                        {
                                            var viewItem = new TreeViewItem()
                                            {
                                                Header = fi.Name
                                            };
                                            tree.Items.Add(viewItem);
                                        }
                                    }
                                    else
                                    {
                                        var viewItem = new TreeViewItem();
                                        viewItem.Header = fi.Name;
                                        viewItem.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(mouse_Down));
                                        viewItem.DataContext = fi.FullName;
                                        tree.Items.Add(viewItem);
                                    }
                                    count++;
                                    txtLog.Text = $"Название файла: {fi.Name}\nКоличество файлов: {count}\nВремя: {sec} сек.";
                                }
                            }
                        }), DispatcherPriority.Background);
                    }
                }), DispatcherPriority.Normal);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    btnSearch.IsEnabled = true;
                    btnCancel.IsEnabled = false;
                    txtStatus.Text = "Завершено";
                    sec = 0;
                    isWorked = false;
                    count = 0;
                    if (tree.Items.Count == 0)
                        System.Windows.MessageBox.Show("Файлы не найдены");
                }), DispatcherPriority.ApplicationIdle);
            });
        }
        #endregion

        #region CheckBox
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isCheck = true;
            txtText.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheck = false;
            txtText.IsEnabled = false;
        }
        #endregion

        #region Buttons
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            open = fbd.SelectedPath;
            txtOpen.Text = open;
            if (txtOpen.Text != "")
                btnSearch.IsEnabled = true;
            else
                btnSearch.IsEnabled = false;
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            list.Clear();
            tree.Items.Clear();
            isStopped = false;
            isWorked = true;
            btnSearch.IsEnabled = false;
            btnCancel.IsEnabled = true;
            btnPause.IsEnabled = true;
            txtStatus.Text = "Работает";
            Thread thread = new Thread(timerStart);
            thread.Start();
            await Task.Factory.StartNew(Searching);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isStopped = true;
            txtStatus.Text = "Остановлено";
            btnSearch.IsEnabled = true;
            btnCancel.IsEnabled = false;
            sec = 0;
            isWorked = false;
            count = 0;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Feature will be added later.");
        }

        /// <summary>
        /// Open file from tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouse_Down(object sender, MouseButtonEventArgs e)
        {
            String st = (sender as TreeViewItem).DataContext as String;
            Process.Start(st);
        }
        #endregion

        #region Settings
        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Name = txtName.Text;
            Properties.Settings.Default.Path = txtOpen.Text;
            Properties.Settings.Default.Check = Convert.ToBoolean(cb.IsChecked);
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Timer
        private async void timerStart()
        {
            while (isWorked)
            {
                sec++;
                await Task.Delay(1000);
            }
        }
        #endregion
    }
}

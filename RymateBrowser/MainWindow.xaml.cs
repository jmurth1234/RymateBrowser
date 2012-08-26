using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Awesomium.Windows.Controls;
using System.Collections;

namespace RymateBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        Hashtable tabs = new Hashtable();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void addNewTab()
        {
            //Lets generate a random ID for the tab. It should be good for about 10000 tabs a session.
            Random random = new Random();
            int randomNumber = random.Next(0, 10000);

            CloseableTabItem item = new CloseableTabItem();
            item.Header = "New Tab";

            item.SetBrowserTabId(randomNumber);
            WebControl ctrl = new WebControl();
            item.Content = ctrl;
            ctrl.Width = item.Width;
            ctrl.Height = item.Height;
            ctrl.LoadURL("http://google.com");
            tabs[randomNumber] = item;
            browserTabs.Items.Add(item);

            ctrl.PageContentsReceived += 
                new Awesomium.Core.PageContentsReceivedEventHandler(Browser_PageContentsReceived);

            ctrl.BeginLoading +=
                new Awesomium.Core.BeginLoadingEventHandler(Browser_BeginLoading);

            item.CloseTab +=
                new RoutedEventHandler(CloseTab_Click);

            browserTabs.SelectedItem = item;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                // Get the current window handle
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;

                this.Background = Brushes.Transparent;

                // Set the proper margins for the extended glass part
                MARGINS margins = new MARGINS();
                margins.cxLeftWidth = -1;
                margins.cxRightWidth = -1;
                margins.cyTopHeight = -1;
                margins.cyBottomHeight = -1;

                int result = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);

                if (result < 0)
                {
                    MessageBox.Show("An error occured while extending the glass unit.");
                }
            }
            addNewTab();
            //webBrowser1.LoadURL("http://google.co.uk");
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!textBox1.Text.Contains(" "))
                {
                    getCurrentBrowser().LoadURL(textBox1.Text);
                }
                else
                {
                    getCurrentBrowser().LoadURL("https://www.google.co.uk/search?q=" + textBox1.Text);
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            getCurrentBrowser().GoBack();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            getCurrentBrowser().GoForward();
        }

        private void Browser_PageContentsReceived(object sender, Awesomium.Core.GetPageContentsEventArgs e)
        {
            String url = e.Url;
            textBox1.Text = url;
            TabItem tab = (TabItem)browserTabs.SelectedItem;
            tab.Header = getCurrentBrowser().Title;
            MyWindow.Title = getCurrentBrowser().Title;
        }

        private void Browser_BeginLoading(object sender, Awesomium.Core.BeginLoadingEventArgs e)
        {
            String url = e.Url;
            textBox1.Text = url;
            TabItem tab = (TabItem)browserTabs.SelectedItem;
            tab.Header = getCurrentBrowser().Title;
            MyWindow.Title = getCurrentBrowser().Title;
        }

        private void newTab_Click(object sender, RoutedEventArgs e)
        {
            addNewTab();
        }

        private WebControl getCurrentBrowser()
        {
            return (WebControl)browserTabs.SelectedContent;
        }

        private void browserTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (browserTabs.Items.IsEmpty)
            {
                this.Close();
                Environment.Exit(0);
            }

            TabItem tab = (TabItem)browserTabs.SelectedItem;

            WebControl web = (WebControl)tab.Content;

            if (web != null)
            {
                if (web.Title != null)
                {
                    MyWindow.Title = web.Title;
                }

                if (web.Source != null)
                {
                    textBox1.Text = web.Source.ToString();
                }

                web.Width = tab.Width;
                web.Height = tab.Height;
            }
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = e.Source as TabItem;
            if (tabItem != null)
            {
                TabControl tabControl = tabItem.Parent as TabControl;
                if (tabControl != null)
                    tabControl.Items.Remove(tabItem);
            }

        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public class MARGINS
    {
        public int cxLeftWidth, cxRightWidth,
            cyTopHeight, cyBottomHeight;
    }
}

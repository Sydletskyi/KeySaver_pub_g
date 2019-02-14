using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RememberMe_testSel.Properties;
using OpenQA.Selenium.Support.UI;
using RememberMe_testSel;

namespace KeySaver
{
    public partial class MainWindow : Window
    {
        #region Fields
        static IWebDriver driver;
        static IWebElement loginField;
        static IWebElement passField;
        static IWebElement signInButton;

        static IWebElement getKeyButton;
        static IWebElement key;
        static IWebElement iFrame;

        string txtbox1, pwdbox;

        string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Shared\TWT_key.txt";

        string urlMain = "url here";
        string urlKey = "url here";

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            passwordBox.PasswordChar = '*';

            if (Settings.Default["Login"] != null)
            {
                TextBox1.Text = Settings.Default.Login;
            }
            if (Settings.Default["Password"] != null)
            {
                passwordBox.Password = Settings.Default.Password;
            }
            if (Settings.Default.RememberMe != false)
            {
                rememberMeChk.IsChecked = true;
            }
        }

        private void ChromeDriverStart()
        {
            try
            {
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                //ChromeOptions option = new ChromeOptions();
                //option.AddArguments("--headless", "--window-size=1920,1080");
                driver = new ChromeDriver(service); // option
                //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                driver.Navigate().GoToUrl(urlMain);
            }
            catch (InvalidOperationException)
            {
                driver.Close();
                MessageBox.Show("Please, use latest version of Chrome");
            }
        }

        private void GettingKey()
        {
            //this.Dispatcher.Invoke(() =>
            //{
            //    RememberMeUpdate();
            //});
            
            driver.Navigate().GoToUrl(urlKey);

            getKeyButton = driver.FindElement(By.Id("submitBtn"));
            getKeyButton.Click();

            iFrame = driver.FindElement(By.XPath("//html//body//iframe"));
            driver.SwitchTo().Frame(iFrame);

            key = driver.FindElement(By.Id("keyInput"), 15);

            key.GetAttribute("value");

            File.WriteAllText(path, key.GetAttribute("value"));

            driver.Quit();

            this.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        void ButtonClick()
        {
            ChromeDriverStart();

            this.Dispatcher.Invoke(() =>
            {
                txtbox1 = TextBox1.Text;
                pwdbox = passwordBox.Password;
            });

            if (!String.IsNullOrEmpty(txtbox1))
            {
                if (!String.IsNullOrEmpty(pwdbox))
                {
                    //Alert
                    //driver.SwitchTo().Alert().Dismiss();
                    //isAlertPresent();

                    loginField = driver.FindElement(By.Name("T1"));
                    loginField.SendKeys(txtbox1);

                    passField = driver.FindElement(By.Name("T2"));
                    passField.SendKeys(pwdbox);

                    signInButton = driver.FindElement(By.Name("B1"));
                    signInButton.Click();

                    if (driver.FindElements(By.Name("B1")).Count != 0)
                    {
                        driver.Close();
                        Settings.Default.Login = null;
                        Settings.Default.Password = null;
                        MessageBox.Show("Wrong credentials !");
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            RememberMeUpdate();
                        });
                        GettingKey();
                    }
                }
                else
                {
                    MessageBox.Show("Enter a password");
                }
            }
            else
            {
                MessageBox.Show("Enter a login");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ButtonClick());
        }

        #region Checkbox

        void RememberMeUpdate()
        {
            Settings.Default["Login"] = TextBox1.Text;
            Settings.Default["Password"] = passwordBox.Password;
            if (Settings.Default.RememberMe == true)
                Settings.Default.Save();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["Login"] = TextBox1.Text;
            Settings.Default["Password"] = passwordBox.Password;
            Settings.Default.RememberMe = true;
            Settings.Default.Save();
        }

        private void rememberMeChk_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
        }

        #endregion
    }

    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
    }
}
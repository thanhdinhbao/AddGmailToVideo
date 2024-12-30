using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace AddGmailToVideo
{
    public partial class fMain : Form
    {
        public fMain()
        {
            InitializeComponent();
        }

        private void LoadChromeProfiles()
        {
            string chromeUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data");

            if (Directory.Exists(chromeUserDataPath))
            {
                var profiles = Directory.GetDirectories(chromeUserDataPath, "Profile*", SearchOption.TopDirectoryOnly);
                profiles = profiles.Prepend(Path.Combine(chromeUserDataPath, "Default")).ToArray();

                foreach (var profilePath in profiles)
                {
                    string profileName = Path.GetFileName(profilePath);
                    cbxProfiles.Items.Add(profileName);
                }

                if (cbxProfiles.Items.Count > 0)
                {
                    cbxProfiles.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy thư mục profile của Chrome!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ReadExcelColumn(string filePath, int columnIndex)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string result2;
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                bool flag = worksheet == null;
                if (flag)
                {
                    throw new Exception("Không tìm thấy worksheet nào trong file Excel.");
                }
                string result = "";
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string cellValue = worksheet.Cells[row, columnIndex].Text;
                    result = result + cellValue + ", ";
                }
                bool flag2 = result.EndsWith(", ");
                if (flag2)
                {
                    result = result.Substring(0, result.Length - 2);
                }
                result2 = result;
            }
            return result2;
        }

        void OpenChrome()
        {
            try
            {
                string chromeUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data");
                string select_profile = cbxProfiles.SelectedItem?.ToString() ?? throw new Exception("Profile not selected.");
                string ProfilePath = Path.Combine(chromeUserDataPath, select_profile);

                if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
                    throw new Exception("Invalid file path.");

                ChromeDriverService cService = ChromeDriverService.CreateDefaultService();
                cService.HideCommandPromptWindow = true;

                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--window-size=1200,800");
                options.AddArgument("--disable-infobars");
                options.AddArgument("--start-maximized");
                options.AddExcludedArgument("enable-automation");
                options.AddArgument($"--user-data-dir={chromeUserDataPath}");
                options.AddArgument($"--profile-directory={select_profile}");
                options.AddArguments("no-sandbox", "disable-gpu");

                using (ChromeDriver driver = new ChromeDriver(cService, options))
                {
                    driver.Navigate().GoToUrl("https://studio.youtube.com/");
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    ProcessVideos(driver, wait, txtFilePath.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        void ProcessVideos(ChromeDriver driver, WebDriverWait wait, string filePath)
        {
            try
            {
                for (int page = 0; page < 2; page++) // Example for handling 2 pages
                {
                    for (int i = 0; i < 31; i++)
                    {
                        try
                        {
                            string script = $"document.getElementsByClassName('label-span style-scope ytcp-video-row')[{i}].click()";
                            ((IJavaScriptExecutor)driver).ExecuteScript(script);

                            IWebElement radPrivate = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#offRadio")));
                            radPrivate.Click();

                            IWebElement btnAddUser = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#privacy-radios > div.private-share-area.style-scope.ytcp-video-visibility-select > ytcp-button > ytcp-button-shape > button")));
                            btnAddUser.Click();

                            string data = ReadExcelColumn(filePath, 1);
                            IWebElement input = driver.FindElement(By.XPath("/html/body/ytcp-private-video-sharing-dialog/ytcp-dialog/tp-yt-paper-dialog/div[2]/div/ytcp-form-input-container/div[1]/div[2]/ytcp-chip-bar/div/input"));
                            input.SendKeys(data);

                            driver.FindElement(By.CssSelector("#done-button > ytcp-button-shape > button")).Click();
                            IWebElement btnSave = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#save-button > ytcp-button-shape > button")));
                            btnSave.Click();

                            driver.Navigate().Refresh();
                        }
                        catch (NoSuchElementException ex)
                        {
                            MessageBox.Show($"Error processing video {i + 1}: {ex.Message}");
                        }
                    }

                    try
                    {
                        IWebElement btnNextPage = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//ytcp-icon-button[@aria-label='Next page']")));
                        btnNextPage.Click();
                    }
                    catch
                    {
                        break; // No more pages
                    }
                }
                MessageBox.Show("DONE!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Processing error: " + ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Excel File";
            openFileDialog.Filter = "Excel files|*.xlsx";
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.RestoreDirectory = true;
            bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
            bool flag2 = flag;
            if (flag2)
            {
                this.txtFilePath.Text = openFileDialog.FileName;
                this.btnStart.Enabled = true;
            }
        }


        private void fMain_Load(object sender, EventArgs e)
        {
            LoadChromeProfiles();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            OpenChrome();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}

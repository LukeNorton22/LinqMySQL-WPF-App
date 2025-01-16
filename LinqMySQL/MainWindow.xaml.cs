using System.Windows;
using LinqMySQL.Services;

namespace LinqMySQL
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FormatSqlButton_Click(object sender, RoutedEventArgs e)
        {
            string rawSql = RawSqlTextBox.Text;
            string output = "";

            if (OutputTypeComboBox.SelectedIndex == 0) // Formatted SQL
            {
                output = SqlFormatter.Format(rawSql); // Format SQL
            }
            else if (OutputTypeComboBox.SelectedIndex == 1) // LINQ Query
            {
                var parsedSql = SqlParser.Parse(rawSql); // Parse SQL into a structured format
                output = SqlToLinq.ConvertSqlToLinq(parsedSql); // Convert to LINQ
            }

            FormattedSqlTextBox.Text = output;
        }

        private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Copy formatted SQL to clipboard
            Clipboard.SetText(FormattedSqlTextBox.Text);
            MessageBox.Show(
                "Formatted SQL copied to clipboard!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private string FormatSql(string rawSql)
        {
            // Placeholder logic for SQL formatting
            // Replace with proper formatting later
            return SqlFormatter.Format(rawSql);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverterTutorial_StaticData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter dataAdapter = new SqlDataAdapter();

        private int currencyId = 0;
        private double fromAmount = 0;
        private double toAmount = 0;


        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }

        public void MyCon()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        private void BindCurrency()
        {
            MyCon();
            
            DataTable dataTable = new DataTable();

            cmd = new SqlCommand("select Id, CurrencyName from Currency_Master", connection);

            cmd.CommandType = CommandType.Text;

            dataAdapter = new SqlDataAdapter(cmd);
            dataAdapter.Fill(dataTable);

            DataRow newRow = dataTable.NewRow();
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";

            dataTable.Rows.InsertAt(newRow,0);

            if(dataTable != null && dataTable.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dataTable.DefaultView;

                cmbToCurrency.ItemsSource = dataTable.DefaultView;
            }
            connection.Close();

            //Sets the combo box to display the right values while retaining proper value
            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }
        public void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
        public void Convert_Click(object sender, RoutedEventArgs e)
        {
            double convertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please enter value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                txtCurrency.Focus();
                return;
            } 
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select 'from' currency", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                cmbFromCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select 'to' currency", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                cmbToCurrency.Focus();
                return;
            }

            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                try
                {
                    convertedValue = double.Parse(txtCurrency.Text);

                    lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N2");
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    convertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * double.Parse(txtCurrency.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());
                    lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N2");
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
               
            }
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if(cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
            }
            if(cmbToCurrency.Items.Count > 0)
            {
                cmbToCurrency.SelectedIndex = 0;
            }
            lblCurrency.Content = "";
            txtCurrency.Focus();
            
        }
        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                currencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public void GetData()
        {
            MyCon();
            DataTable dataTable = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Currency_Master", connection);
            cmd.CommandType = CommandType.Text;
            dataAdapter = new SqlDataAdapter(cmd);
            dataAdapter.Fill(dataTable);
            if(dataTable != null && dataTable.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            connection.Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(txtAmount.Text == null || txtAmount.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter a valid amount", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter a valid currency name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {
                    if (currencyId > 0)
                    {
                        //Update button
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyCon();
                            DataTable dataTable = new DataTable();
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", connection);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", currencyId);
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            connection.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearMaster();
                        }
                    }
                    else 
                    {
                        //Save button
                        if (MessageBox.Show("Are you sure you want to save?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyCon();
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", connection);
                            cmd.CommandType= CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            connection.Close();
   
                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearMaster();
                        }
                    }
                    ClearMaster();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = (DataGrid)sender;
                DataRowView rowSelected = dataGrid.CurrentItem as DataRowView;

                if (rowSelected != null)
                {
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (dataGrid.SelectedCells.Count > 0)
                        {
                            currencyId = Int32.Parse(rowSelected["Id"].ToString());

                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                txtAmount.Text = rowSelected["Amount"].ToString();
                                txtCurrencyName.Text = rowSelected["CurrencyName"].ToString();
                                btnSave.Content = "Update";
                            }
                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if (MessageBox.Show("Are you sure you want to delete?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    MyCon();
                                    DataTable dataTable = new DataTable();
                                    cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", connection);
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.AddWithValue("@Id", currencyId);
                                    cmd.ExecuteNonQuery();
                                    connection.Close();

                                    MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message,"Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

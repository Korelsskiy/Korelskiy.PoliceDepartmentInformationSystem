using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Korelskiy.PoliceDepartmentInformationSystem
{
    public enum CheckStyle
    {
        UserName,
        Password,
        Login,
        PersonInitals,
        OfficerTeam,
        OfficerAdd,
        SuspectCase,
        VictimCase,
        CriminalCase,
        CheckTheSameInDb
    }
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=testPolice;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private SqlConnection connection;
        private SqlCommand cmd;
        private DataSet ds;
        private SqlDataAdapter da;
        private SqlDataReader dr;
        private bool isAutentificated = false;
        public Form1()
        {
            InitializeComponent();
            GetOfficers();
            GetSuspects();
            GetVictims();
            GetCases();
            GetTeams();
            GetOfficersByTeams();
            GetSuspectsByCases();
            GetVictimsByCases();
        }

        private bool CheckFoo(CheckStyle style, object[] testParametrs)
        {
            int caseId = 0;
            string tableTitle = "";
            string columnTitle = "";
            object parametr = new object();
            switch (style)
            {
                case CheckStyle.UserName:
                    string userName = testParametrs[0].ToString();
                    if (userName.Length < 8)
                    {
                        MessageBox.Show("Имя пользователя должно содержать не менее 8 символов!");
                        return false;
                    }
                    break;
                case CheckStyle.Password:
                    string password = testParametrs[0].ToString();
                    if (password.Length < 6)
                    {
                        MessageBox.Show("Пароль должен содержать не менее 6 символов!");
                        return false;
                    }
                    break;
                case CheckStyle.Login:
                    string login = testParametrs[0].ToString();
                    string pass= testParametrs[1].ToString();
                    connection = new SqlConnection(connectionString);
                    try
                    {
                        connection.Open();
                        SqlCommand com = new SqlCommand($"SELECT count(*) FROM Users WHERE UserName=@UserName AND UserPassword=@UserPassword", connection);
                        com.Parameters.AddWithValue("UserName", login);
                        com.Parameters.AddWithValue("UserPassword", pass);
                        int i = Convert.ToInt32(com.ExecuteScalar());
                        if (i == 0)
                        {
                            MessageBox.Show("Такого пользователя не существует!");
                            return false;
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    break;
                case CheckStyle.PersonInitals:
                    foreach (var item in testParametrs)
                    {
                        if (item.ToString().Length < 2)
                        {
                            MessageBox.Show("ФИО заполнены не верно!");
                            return false;
                        }
                    }
                    break;
                case CheckStyle.OfficerTeam:
                    int officerId = 0;
                    int teamId = 0;
                    if (int.TryParse(testParametrs[0].ToString(), out officerId) && (int.TryParse(testParametrs[1].ToString(), out teamId)))
                    {
                        connection = new SqlConnection(connectionString);
                        try
                        {
                            connection.Open();
                            SqlCommand com = new SqlCommand($"SELECT count(*) FROM Officers WHERE Id=@OfficerId", connection);
                            SqlCommand cmd = new SqlCommand($"SELECT count(*) FROM InvestigationTeams WHERE Id=@TeamId", connection);
                            com.Parameters.AddWithValue("OfficerId", officerId);
                            cmd.Parameters.AddWithValue("TeamId", teamId);
                            int i = Convert.ToInt32(com.ExecuteScalar());
                            int j = Convert.ToInt32(cmd.ExecuteScalar());
                            if (i == 0)
                            {
                                MessageBox.Show("Такого офицера нет!");
                                return false;
                            }
                            if (j == 0)
                            {
                                MessageBox.Show("Такой следственной группы нет!");
                                return false;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введены не числа!");
                        return false;
                    }
                    break;
                case CheckStyle.OfficerAdd:
                    if (testParametrs[0].ToString().Length < 5)
                    {
                        MessageBox.Show("Слишком мало символов в должности!");
                        return false;
                    }
                    int salary = 0;
                    if (int.TryParse(testParametrs[1].ToString(), out salary))
                    {
                        if (salary < 25_000)
                        {
                            MessageBox.Show("Зарплата должна быть не меньше 25 000!");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Зарпалата должна быть числом!");
                        return false;
                    }
                    break;
                case CheckStyle.SuspectCase:
                    int suspectId = 0;
                    caseId = 0;
                    if (int.TryParse(testParametrs[0].ToString(), out suspectId) && (int.TryParse(testParametrs[1].ToString(), out caseId)))
                    {
                        connection = new SqlConnection(connectionString);
                        try
                        {
                            connection.Open();
                            SqlCommand com = new SqlCommand($"SELECT count(*) FROM Suspects WHERE Id=@SuspectId", connection);
                            SqlCommand cmd = new SqlCommand($"SELECT count(*) FROM CriminalCases WHERE Id=@CaseId", connection);
                            com.Parameters.AddWithValue("SuspectId", suspectId);
                            cmd.Parameters.AddWithValue("CaseId", caseId);
                            int i = Convert.ToInt32(com.ExecuteScalar());
                            int j = Convert.ToInt32(cmd.ExecuteScalar());
                            if (i == 0)
                            {
                                MessageBox.Show("Такого подозреваемого нет!");
                                return false;
                            }
                            if (j == 0)
                            {
                                MessageBox.Show("Такого дела нет!");
                                return false;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введены не числа!");
                        return false;
                    }
                    break;
                case CheckStyle.VictimCase:
                    int victimId = 0;
                    caseId = 0;
                    if (int.TryParse(testParametrs[0].ToString(), out victimId) && (int.TryParse(testParametrs[1].ToString(), out caseId)))
                    {
                        connection = new SqlConnection(connectionString);
                        try
                        {
                            connection.Open();
                            SqlCommand com = new SqlCommand($"SELECT count(*) FROM Victims WHERE Id=@VictimId", connection);
                            SqlCommand cmd = new SqlCommand($"SELECT count(*) FROM CriminalCases WHERE Id=@CaseId", connection);
                            com.Parameters.AddWithValue("VictimId", victimId);
                            cmd.Parameters.AddWithValue("CaseId", caseId);
                            int i = Convert.ToInt32(com.ExecuteScalar());
                            int j = Convert.ToInt32(cmd.ExecuteScalar());
                            if (i == 0)
                            {
                                MessageBox.Show("Такого потерпевшего нет!");
                                return false;
                            }
                            if (j == 0)
                            {
                                MessageBox.Show("Такого дела нет!");
                                return false;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введены не числа!");
                        return false;
                    }
                    break;
                case CheckStyle.CriminalCase:
                    connection = new SqlConnection(connectionString);
                    string caseTitle = testParametrs[0].ToString();
                    int investigationTeamId = 0;
                    int maximumPenalty = 0;

                    if (caseTitle.Length < 5)
                    {
                        MessageBox.Show("Слишком короткое название преступления!");
                        return false;
                    }

                    if (int.TryParse(testParametrs[1].ToString(), out investigationTeamId) && (int.TryParse(testParametrs[2].ToString(), out maximumPenalty)))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand($"SELECT count(*) FROM InvestigationTeams WHERE Id=@TeamId", connection);
                        cmd.Parameters.AddWithValue("TeamId", investigationTeamId);
                        int j = Convert.ToInt32(cmd.ExecuteScalar());
                        if (j == 0)
                        {
                            MessageBox.Show("Такой сл.группы не существует!");
                            return false;
                        }

                        if (maximumPenalty < 1)
                        {
                            MessageBox.Show("Максимальный срок не может быть меньше 1!");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("В числовых полях введены не числа!");
                        return false;
                    }
                    break;
                case CheckStyle.CheckTheSameInDb:
                    connection = new SqlConnection(connectionString);
                    tableTitle = testParametrs[0].ToString();
                    columnTitle = testParametrs[1].ToString();
                    parametr = testParametrs[2];
                    try
                    {
                        connection.Open();
                        if (tableTitle == "Users")
                        {
                            SqlCommand com = new SqlCommand($"SELECT count(*) FROM {tableTitle} WHERE {columnTitle}=@{columnTitle}", connection);
                            com.Parameters.AddWithValue(columnTitle, parametr);
                            int i = Convert.ToInt32(com.ExecuteScalar());
                            if (i != 0)
                            {
                                MessageBox.Show("Пользователь с таким логином уже зарегистрирован.");
                                return false;
                            }
                        }
                        else
                        {
                            string column2Title = testParametrs[3].ToString();
                            object parametr2 = testParametrs[4];
                            SqlCommand com = new SqlCommand($"SELECT count(*) FROM {tableTitle} WHERE {columnTitle}=@{columnTitle} AND {column2Title}=@{column2Title}", connection);
                            com.Parameters.AddWithValue(columnTitle, parametr);
                            com.Parameters.AddWithValue(column2Title, parametr2);
                            int i = Convert.ToInt32(com.ExecuteScalar());
                            if (i != 0)
                            {
                                if (tableTitle == "OfficersByTeams")
                                {
                                    MessageBox.Show("Этот офицер уже в этой группе!");
                                }
                                if (tableTitle == "SuspectsByCases")
                                {
                                    MessageBox.Show("Этот подозреваемый уже в этой группе!");
                                }
                                if (tableTitle == "VictimsByCases")
                                {
                                    MessageBox.Show("Этот потерпевший уже в этой группе!");
                                }
                                return false;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
        private void GetOfficers()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM Officers", connection);
                ds = new DataSet();
                da.Fill(ds);
                officersDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetSuspects()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM Suspects", connection);
                ds = new DataSet();
                da.Fill(ds);
                suspectDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetVictims()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM Victims", connection);
                ds = new DataSet();
                da.Fill(ds);
                victimDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetCases()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM CriminalCases", connection);
                ds = new DataSet();
                da.Fill(ds);
                criminalCasesDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetTeams()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM InvestigationTeams", connection);
                ds = new DataSet();
                da.Fill(ds);
                teamsDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetOfficersByTeams()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM OfficersByTeams", connection);
                ds = new DataSet();
                da.Fill(ds);
                officersByTeamsDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetSuspectsByCases()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM SuspectsByCases", connection);
                ds = new DataSet();
                da.Fill(ds);
                suspectByCasesDataFridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void GetVictimsByCases()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter("SELECT * FROM VictimsByCases", connection);
                ds = new DataSet();
                da.Fill(ds);
                victimByCasesDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void addOfficerButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] { firstNameTextBox.Text, lastNameTextBox.Text, passwordTextBox.Text})
                && CheckFoo(CheckStyle.OfficerAdd, new object[] { positionTextBox.Text, salaryTextBox.Text}))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO Officers(FirstName, LastName, Patronymic, Position, Salary, Birthday) " +
                        $"VALUES(@FirstName, @LastName, @Patronymic, @Position, @Salary, @Birthday)", connection);
                    cmd.Parameters.AddWithValue("FirstName", firstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", lastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", patronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("Position", positionTextBox.Text);
                    cmd.Parameters.AddWithValue("Salary", Convert.ToInt32(salaryTextBox.Text));
                    cmd.Parameters.AddWithValue("Birthday", birthdayDatePicker.Value);
                    cmd.ExecuteNonQuery();
                    GetOfficers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addSuspectButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] {suspectFirstNameTextBox.Text, suscpectLastNameTextBox.Text, suspectPatronymicTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO Suspects(FirstName, LastName, Patronymic, Birthday) " +
                        $"VALUES(@FirstName, @LastName, @Patronymic, @Birthday)", connection);
                    cmd.Parameters.AddWithValue("FirstName", suspectFirstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", suscpectLastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", suspectPatronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("Birthday", suspectBirthdayDatePicker.Value);
                    cmd.ExecuteNonQuery();
                    GetSuspects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addVictimButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] {victimFirstNameTextBox.Text, victimLastNameTextBox.Text, victimPatronymicTextBox.Text}))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO Victims(FirstName, LastName, Patronymic, RequestDate, Birthday) " +
                        $"VALUES(@FirstName, @LastName, @Patronymic, @RequestDate, @Birthday)", connection);
                    cmd.Parameters.AddWithValue("FirstName", victimFirstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", victimLastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", victimPatronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("RequestDate", requestDateDatePicker.Value);
                    cmd.Parameters.AddWithValue("Birthday", victimBirthdayDatePicker.Value);
                    cmd.ExecuteNonQuery();
                    GetVictims();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.CriminalCase, new object[] {punishmentTitleTextBox.Text, investigationTeamIdTextBox.Text, maximumPenaltyTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO CriminalCases(InvestigationTeamId, MaximumPenalty, PunishmentTitle) " +
                        $"VALUES(@InvestigationTeamId, @MaximumPenalty, @PunishmentTitle)", connection);
                    cmd.Parameters.AddWithValue("InvestigationTeamId", investigationTeamIdTextBox.Text);
                    cmd.Parameters.AddWithValue("MaximumPenalty", maximumPenaltyTextBox.Text);
                    cmd.Parameters.AddWithValue("PunishmentTitle", punishmentTitleTextBox.Text);
                    cmd.ExecuteNonQuery();
                    GetCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand($"INSERT INTO InvestigationTeams(CreationDate) " +
                    $"VALUES(@CreationDate)", connection);
                cmd.Parameters.AddWithValue("CreationDate", DateTime.Now.Date);
                cmd.ExecuteNonQuery();
                GetTeams();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void editOfficerButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] { firstNameTextBox.Text, lastNameTextBox.Text, passwordTextBox.Text })
                && CheckFoo(CheckStyle.OfficerAdd, new object[] { positionTextBox.Text, salaryTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE Officers SET FirstName=@FirstName, LastName=@LastName, Patronymic=@Patronymic, Position=@Position, Salary=@Salary, Birthday=@Birthday WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("FirstName", firstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", lastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", patronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("Position", positionTextBox.Text);
                    cmd.Parameters.AddWithValue("Salary", Convert.ToInt32(salaryTextBox.Text));
                    cmd.Parameters.AddWithValue("Birthday", birthdayDatePicker.Value);
                    cmd.Parameters.AddWithValue("Id", officersDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                GetOfficers();
            }
        }

        private void editSuspectButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] { suspectFirstNameTextBox.Text, suscpectLastNameTextBox.Text, suspectPatronymicTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE Suspects SET FirstName=@FirstName, LastName=@LastName, Patronymic=@Patronymic, Birthday=@Birthday WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("FirstName", suspectFirstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", suscpectLastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", suspectPatronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("Birthday", suspectBirthdayDatePicker.Value);
                    cmd.Parameters.AddWithValue("Id", suspectDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                    GetSuspects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
                
        }

        private void editVictimButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.PersonInitals, new object[] { victimFirstNameTextBox.Text, victimLastNameTextBox.Text, victimPatronymicTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE Victims SET FirstName=@FirstName, LastName=@LastName, Patronymic=@Patronymic, RequestDate=@RequestDate, Birthday=@Birthday WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("FirstName", victimFirstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("LastName", victimLastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("Patronymic", victimPatronymicTextBox.Text);
                    cmd.Parameters.AddWithValue("RequestDate", requestDateDatePicker.Value);
                    cmd.Parameters.AddWithValue("Birthday", victimBirthdayDatePicker.Value);
                    cmd.Parameters.AddWithValue("Id", victimDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                    GetVictims();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void editCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.CriminalCase, new object[] { punishmentTitleTextBox.Text, investigationTeamIdTextBox.Text, maximumPenaltyTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE CriminalCases SET InvestigationTeamId=@InvestigationTeamId, MaximumPenalty=@MaximumPenalty, PunishmentTitle=@PunishmentTitle WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("InvestigationTeamId", investigationTeamIdTextBox.Text);
                    cmd.Parameters.AddWithValue("MaximumPenalty", maximumPenaltyTextBox.Text);
                    cmd.Parameters.AddWithValue("PunishmentTitle", punishmentTitleTextBox.Text);
                    cmd.Parameters.AddWithValue("Id", criminalCasesDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                    GetCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
                
        }

        private void officersDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                firstNameTextBox.Text = officersDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                lastNameTextBox.Text = officersDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                patronymicTextBox.Text = officersDataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
                positionTextBox.Text = officersDataGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                salaryTextBox.Text = officersDataGridView.Rows[e.RowIndex].Cells[5].Value.ToString();
                birthdayDatePicker.Value = Convert.ToDateTime(officersDataGridView.Rows[e.RowIndex].Cells[6].Value);
            }
            
        }

        private void suspectDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                suspectFirstNameTextBox.Text = suspectDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                suscpectLastNameTextBox.Text = suspectDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                suspectPatronymicTextBox.Text = suspectDataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
                suspectBirthdayDatePicker.Value = Convert.ToDateTime(suspectDataGridView.Rows[e.RowIndex].Cells[4].Value);
            }
            
        }
        private void victimDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                victimFirstNameTextBox.Text = victimDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                victimLastNameTextBox.Text = victimDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                victimPatronymicTextBox.Text = victimDataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
                requestDateDatePicker.Value = Convert.ToDateTime(victimDataGridView.Rows[e.RowIndex].Cells[4].Value);
                victimBirthdayDatePicker.Value = Convert.ToDateTime(victimDataGridView.Rows[e.RowIndex].Cells[5].Value);
            }
            
        }

        private void criminalCasesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                investigationTeamIdTextBox.Text = criminalCasesDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                maximumPenaltyTextBox.Text = criminalCasesDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                punishmentTitleTextBox.Text = criminalCasesDataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
            }
        }

        private void officersByTeamsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                teamIdTextBox.Text = officersByTeamsDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                officerIdTextBox.Text = officersByTeamsDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
            }
        }

        private void suspectByCasesDataFridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                caseIdTextBox.Text = suspectByCasesDataFridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                suspectIdTextBox.Text = suspectByCasesDataFridView.Rows[e.RowIndex].Cells[2].Value.ToString();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                victimCaseIdTextBox.Text = victimByCasesDataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                victimIdTextBox.Text = victimByCasesDataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
            }
        }

        private void deleteOfficerButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM Officers WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", officersDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetOfficers();
        }

        private void deleteSuspectButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM Suspects WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", suspectDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetSuspects();
        }

        private void deleteVictimButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM Victims WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", victimDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetVictims();
        }

        private void deleteCaseButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM CriminalCases WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", criminalCasesDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetCases();
        }

        private void deleteTeamButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM InvestigationTeams WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", teamsDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetTeams();
        }
        private void filtrButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter($"SELECT * FROM Officers WHERE LastName LIKE N'%{filtrTextBox.Text}%'", connection);
                ds = new DataSet();
                da.Fill(ds);
                officersDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void suspectFiltrButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter($"SELECT * FROM Suspects WHERE LastName LIKE N'%{suspectFiltrTextBox.Text}%'", connection);
                ds = new DataSet();
                da.Fill(ds);
                suspectDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void victimFiltrButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter($"SELECT * FROM Victims WHERE LastName LIKE N'%{victimFiltrTextBox.Text}%'", connection);
                ds = new DataSet();
                da.Fill(ds);
                victimDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void caseFiltrButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                da = new SqlDataAdapter($"SELECT * FROM CriminalCases WHERE PunishmentTitle LIKE N'%{caseFiltrTextBox.Text}%'", connection);
                ds = new DataSet();
                da.Fill(ds);
                criminalCasesDataGridView.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            GetOfficers();
        }

        private void suspectDatabaseUpdateButton_Click(object sender, EventArgs e)
        {
            GetSuspects();
        }

        private void victimDatabaseUpdate_Click(object sender, EventArgs e)
        {
            GetVictims();
        }

        private void caseDatabaseUpdateButton_Click(object sender, EventArgs e)
        {
            GetCases();
        }

        private void signInButton_Click(object sender, EventArgs e)
        {
            statusPanel.Visible = false;
            if (CheckFoo(CheckStyle.Login, new object[] { userNameTextBox.Text, passwordTextBox.Text}))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"SELECT * FROM Users WHERE UserName = N'{userNameTextBox.Text}'", connection);
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            userNameLabel.Text = dr["UserName"].ToString();
                            isAutentificated = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.Password, new object[] { passwordTextBox.Text }) 
                && CheckFoo(CheckStyle.UserName, new object[] { userNameTextBox.Text })
                && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "Users", "UserName", userNameTextBox.Text}))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO Users(UserName, UserPassword) " +
                        $"VALUES(@UserName, @UserPassword)", connection);
                    cmd.Parameters.AddWithValue("UserName", userNameTextBox.Text);
                    cmd.Parameters.AddWithValue("UserPassword", passwordTextBox.Text);
                    cmd.ExecuteNonQuery();
                    statusPanel.BackColor = Color.LightGreen;
                    statusPanelLabel.Visible = true;
                    statusPanelLabel.Text = "Вы зарегистрированы";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addOfficerToTeamButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.OfficerTeam, new object[] {officerIdTextBox.Text, teamIdTextBox.Text})
                && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "OfficersByTeams", "OfficerId", officerIdTextBox.Text, "TeamId", teamIdTextBox.Text}))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO OfficersByTeams(TeamId, OfficerId) " +
                        $"VALUES(@TeamId, @OfficerId)", connection);
                    cmd.Parameters.AddWithValue("TeamId", teamIdTextBox.Text);
                    cmd.Parameters.AddWithValue("OfficerId", officerIdTextBox.Text);
                    cmd.ExecuteNonQuery();
                    GetOfficersByTeams();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void addSuspectToCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.SuspectCase, new object[] {suspectIdTextBox.Text, caseIdTextBox.Text})
                && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "SuspectsByCases", "SuspectId", suspectIdTextBox.Text, "CriminalCaseId", caseIdTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO SuspectsByCases(SuspectId, CriminalCaseId) " +
                        $"VALUES(@SuspectId, @CriminalCaseId)", connection);
                    cmd.Parameters.AddWithValue("SuspectId", suspectIdTextBox.Text);
                    cmd.Parameters.AddWithValue("CriminalCaseId", caseIdTextBox.Text);
                    cmd.ExecuteNonQuery();
                    GetSuspectsByCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }   
        }

        private void addVictimToCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.VictimCase, new object[] {victimIdTextBox.Text, victimCaseIdTextBox.Text })
                && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "VictimsByCases", "VictimId", victimIdTextBox.Text, "CriminalCaseId", victimCaseIdTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand($"INSERT INTO VictimsByCases(VictimId, CriminalCaseId) " +
                        $"VALUES(@VictimId, @CriminalCaseId)", connection);
                    cmd.Parameters.AddWithValue("VictimId", victimIdTextBox.Text);
                    cmd.Parameters.AddWithValue("CriminalCaseId", victimCaseIdTextBox.Text);
                    cmd.ExecuteNonQuery();
                    GetVictimsByCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM OfficersByTeams WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", officersByTeamsDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            GetOfficersByTeams();
        }

        private void deleteSuspectFromCaseButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM SuspectsByCases WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", suspectByCasesDataFridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
                GetSuspectsByCases();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void deleteVicimFromCaseButton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                cmd = new SqlCommand("DELETE FROM VictimsByCases WHERE Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", victimByCasesDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
                GetVictimsByCases();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void updateTeamButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.OfficerTeam, new object[] { officerIdTextBox.Text, teamIdTextBox.Text })
                 && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "OfficersByTeams", "OfficerId", officerIdTextBox.Text, "TeamId", teamIdTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE OfficersByTeams SET TeamId=@TeamId WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("TeamId", teamIdTextBox.Text);
                    cmd.Parameters.AddWithValue("Id", officersByTeamsDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                GetOfficersByTeams();
            }
            
        }

        private void changeSuspectCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.SuspectCase, new object[] { suspectIdTextBox.Text, caseIdTextBox.Text })
                 && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "SuspectsByCases", "SuspectId", suspectIdTextBox.Text, "CriminalCaseId", caseIdTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE SuspectsByCases SET CriminalCaseId=@CriminalCaseId WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("CriminalCaseId", caseIdTextBox.Text);
                    cmd.Parameters.AddWithValue("Id", suspectByCasesDataFridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                    GetSuspectsByCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
                
        }

        private void changeVictimCaseButton_Click(object sender, EventArgs e)
        {
            if (CheckFoo(CheckStyle.VictimCase, new object[] { victimIdTextBox.Text, victimCaseIdTextBox.Text })
                && CheckFoo(CheckStyle.CheckTheSameInDb, new object[] { "VictimsByCases", "VictimId", victimIdTextBox.Text, "CriminalCaseId", victimCaseIdTextBox.Text }))
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    cmd = new SqlCommand("UPDATE VictimsByCases SET CriminalCaseId=@CriminalCaseId WHERE Id=@Id", connection);
                    cmd.Parameters.AddWithValue("CriminalCaseId", victimCaseIdTextBox.Text);
                    cmd.Parameters.AddWithValue("Id", victimByCasesDataGridView.SelectedRows[0].Cells[0].Value.ToString());
                    cmd.ExecuteNonQuery();
                    GetVictimsByCases();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
                
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            statusPanel.Visible = false;
            statusPanelLabel.Visible = false;
            if (!isAutentificated)
            {
                tabControl1.SelectedTab = tabControl1.TabPages[0];
            }
        }

        private void signOutButton_Click(object sender, EventArgs e)
        {
           
            userNameLabel.Text = "Не авторизован";
            isAutentificated = false;

            statusPanel.Visible = true;
            statusPanel.BackColor = Color.LightYellow;
            statusPanelLabel.Visible = true;
            statusPanelLabel.Text = "Вы вышли из аккаунта";
        }
    }
}

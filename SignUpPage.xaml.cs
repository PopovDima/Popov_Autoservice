using System;
using System.Collections.Generic;
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

namespace Popov_Autoservice
{
    /// <summary>
    /// Логика взаимодействия для SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        private Service _currentService = new Service();
        public SignUpPage(Service SelectedService)
        {
            InitializeComponent();
            if (SelectedService != null)
            {
                this._currentService = SelectedService;
            }

            DataContext = _currentService;

            var _currentClient = Popov_AutoserviceEntities1.GetContext().Client.ToList();
            ComboClient.ItemsSource = _currentClient;
        }

        private ClientService _currentClientService = new ClientService();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (ComboClient.SelectedItem == null)
            {
                errors.AppendLine("Укажите ФИО клиента");
            }
            if (StartDate.Text == "")
            {
                errors.AppendLine("Укажите дату услуги");
            }
            if (TBStart.Text == "")
            {
                errors.AppendLine("Укажите время начала услуги");
            }
            if (!Regex.IsMatch(TBStart.Text, @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$"))
            {
                errors.AppendLine("Укажите корректное время начала услуги");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            //_currentClientService.ClientID = ComboClient.SelectedIndex + 1;

            Client selectedClient = ComboClient.SelectedItem as Client;
            _currentClientService.ClientID = selectedClient.ID;

            _currentClientService.ServiceID = _currentService.ID;
            _currentClientService.StartTime = Convert.ToDateTime(StartDate.Text + " " + TBStart.Text);

            if (_currentClientService.ID == 0)
            {
                Popov_AutoserviceEntities1.GetContext().ClientService.Add(_currentClientService);
            }

            try
            {
                Popov_AutoserviceEntities1.GetContext().SaveChanges();
                MessageBox.Show("информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void TBStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = TBStart.Text;

            // Пытаемся распознать введенный текст как время (формат чч:мм)
            if (TimeSpan.TryParse(s, out TimeSpan startTime))
            {
                // Проверяем, чтобы введенное время было в пределах 24 часов
                if (startTime.TotalHours >= 24)
                {
                    TBEnd.Text = "Ошибка: > 24ч";
                    return;
                }

                TimeSpan duration = TimeSpan.FromMinutes(_currentService.Duration);
                TimeSpan endTime = startTime.Add(duration);

                // Если сумма превышает 24 часа, используем остаток от деления (переход на следующие сутки)
                if (endTime.TotalHours >= 24)
                {
                    endTime = TimeSpan.FromTicks(endTime.Ticks % TimeSpan.FromDays(1).Ticks);
                }

                // Выводим результат в формате ЧЧ:мм с ведущими нулями
                TBEnd.Text = endTime.ToString(@"hh\:mm");
            }
            else
            {
                // Если формат неверный или поле не заполнено полностью
                TBEnd.Text = "";
            }
        }
    }
}

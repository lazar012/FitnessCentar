using System.IO;
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

namespace FitnesCentar38cet
{
    public partial class MainWindow : Window
    {
        private const string FAJL_SA_PODACIMA = @".\clanovi_fitnes_centra.csv";

        private List<Clan> listaClanova = new List<Clan>();

        public MainWindow()
        {
            InitializeComponent(); 
            UcitajPodatkeClanova(); 
            PodesiInterfejs();      
        }

        private void PodesiInterfejs()
        {
            cmbTipClanarine.SelectedIndex = 0;
            
            txtImePrezime.Focus();
        }

        private void UcitajPodatkeClanova()
        {
            try
            {
                if (File.Exists(FAJL_SA_PODACIMA))
                {
                    var sadrzajFajla = File.ReadAllLines(FAJL_SA_PODACIMA);
                    
                    listaClanova = sadrzajFajla
                        .Where(linija => !string.IsNullOrWhiteSpace(linija)) 
                        .Select(zapis =>
                        {
                            var polja = zapis.Split('|');
                            
                            return new Clan(
                                polja[0],                   
                                polja[1],                    
                                polja[2],                    
                                DateTime.Parse(polja[3]),    
                                DateTime.Parse(polja[4])     
                            );
                        }).ToList();

                    OsveziTabeluClanova();
                }
                else
                {
                    File.Create(FAJL_SA_PODACIMA).Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju podataka: {ex.Message}", 
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OsveziTabeluClanova()
        {
            DGCentralni.ItemsSource = null;
            
            DGCentralni.ItemsSource = listaClanova.OrderByDescending(c => c.DatumUpisa).ToList();
        }

        private void Dodaj_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtImePrezime.Text))
            {
                MessageBox.Show("Molimo unesite ime i prezime člana!", 
                    "Greška validacije", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtImePrezime.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTelefon.Text))
            {
                MessageBox.Show("Molimo unesite broj telefona!", 
                    "Greška validacije", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelefon.Focus();
                return;
            }

            string izabranaVrstaClanarine = (cmbTipClanarine.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Mesečna";
            
            DateTime datumIstekaClanske = IzracunajDatumIsteka(izabranaVrstaClanarine);

            var noviClan = new Clan(
                txtImePrezime.Text.Trim(),      
                izabranaVrstaClanarine,
                txtTelefon.Text.Trim(),
                DateTime.Now,                   
                datumIstekaClanske
            );

            try
            {
                listaClanova.Add(noviClan);

                string zapisZaFajl = $"{noviClan.ImePrezime}|{noviClan.TipClanarine}|{noviClan.Telefon}|" +
                                   $"{noviClan.DatumUpisa:yyyy-MM-dd HH:mm:ss}|{noviClan.DatumIsteka:yyyy-MM-dd}\n";
                
                File.AppendAllText(FAJL_SA_PODACIMA, zapisZaFajl);

                MessageBox.Show($"Član {noviClan.ImePrezime} je uspešno registrovan!\n" +
                              $"Članarina važi do: {noviClan.DatumIsteka:dd.MM.yyyy}", 
                    "Uspešna registracija", MessageBoxButton.OK, MessageBoxImage.Information);

                OcistiFormu();

                OsveziTabeluClanova();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri čuvanju člana: {ex.Message}", 
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime IzracunajDatumIsteka(string tipClanarine)
        {
            return tipClanarine switch
            {
                "Mesečna" => DateTime.Now.AddMonths(1),      
                "Kvartalna" => DateTime.Now.AddMonths(3),    
                "Polugodišnja" => DateTime.Now.AddMonths(6), 
                "Godišnja" => DateTime.Now.AddYears(1),      
                "VIP" => DateTime.Now.AddYears(2),           
                _ => DateTime.Now.AddMonths(1)             
            };
        }

        private void Obrisi_Click(object sender, RoutedEventArgs e)
        {
            OcistiFormu();
        }

        private void OcistiFormu()
        {
            txtImePrezime.Clear();            
            txtTelefon.Clear();                 
            cmbTipClanarine.SelectedIndex = 0;  
            txtImePrezime.Focus();            
        }
    }
}

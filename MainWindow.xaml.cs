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
    /// <summary>
    /// Logika interakcije za MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Konstanta koja definiše putanju do fajla sa podacima
        private const string FAJL_SA_PODACIMA = @".\clanovi_fitnes_centra.csv";
        
        // Lista koja čuva sve članove fitnes centra u memoriji
        private List<Clan> listaClanova = new List<Clan>();

        // Konstruktor glavnog prozora - izvršava se kada se aplikacija pokrene
        public MainWindow()
        {
            InitializeComponent();  // Inicijalizuje sve XAML komponente
            UcitajPodatkeClanova(); // Učitava postojeće članove iz fajla
            PodesiInterfejs();      // Postavlja početne vrednosti interfejsa
        }

        // Metoda koja postavlja početno stanje korisničkog interfejsa
        private void PodesiInterfejs()
        {
            // Postavlja prvu opciju u ComboBox-u kao selektovanu
            cmbTipClanarine.SelectedIndex = 0;
            
            // Postavlja fokus na polje za unos imena
            txtImePrezime.Focus();
        }

        // Metoda koja učitava podatke o članovima iz CSV fajla
        private void UcitajPodatkeClanova()
        {
            try
            {
                // Proverava da li fajl sa podacima postoji
                if (File.Exists(FAJL_SA_PODACIMA))
                {
                    // Čita sve linije iz fajla
                    var sadrzajFajla = File.ReadAllLines(FAJL_SA_PODACIMA);
                    
                    // Parsira svaku liniju i kreira objekte tipa Clan
                    listaClanova = sadrzajFajla
                        .Where(linija => !string.IsNullOrWhiteSpace(linija))  // Preskače prazne linije
                        .Select(zapis =>
                        {
                            // Razdvaja podatke po separatoru '|'
                            var polja = zapis.Split('|');
                            
                            // Kreira novi objekat Clan sa učitanim podacima
                            return new Clan(
                                polja[0],                    // Ime i prezime
                                polja[1],                    // Tip članarine
                                polja[2],                    // Telefon
                                DateTime.Parse(polja[3]),    // Datum upisa
                                DateTime.Parse(polja[4])     // Datum isteka
                            );
                        }).ToList();

                    // Osvežava prikaz u DataGrid komponenti
                    OsveziTabeluClanova();
                }
                else
                {
                    // Kreira prazan fajl ako ne postoji
                    File.Create(FAJL_SA_PODACIMA).Close();
                }
            }
            catch (Exception ex)
            {
                // Prikazuje poruku o grešci ako se desi problem pri učitavanju
                MessageBox.Show($"Greška pri učitavanju podataka: {ex.Message}", 
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda koja osvežava prikaz članova u tabeli
        private void OsveziTabeluClanova()
        {
            // Prvo uklanja postojeći izvor podataka
            DGCentralni.ItemsSource = null;
            
            // Postavlja sortiranu listu kao novi izvor podataka (najnoviji članovi prvi)
            DGCentralni.ItemsSource = listaClanova.OrderByDescending(c => c.DatumUpisa).ToList();
        }

        // Event handler za dugme "REGISTRUJ ČLANA"
        private void Dodaj_Click(object sender, RoutedEventArgs e)
        {
            // Validacija - provera da li je upisano ime i prezime
            if (string.IsNullOrWhiteSpace(txtImePrezime.Text))
            {
                MessageBox.Show("Molimo unesite ime i prezime člana!", 
                    "Greška validacije", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtImePrezime.Focus();
                return;
            }

            // Validacija - provera da li je upisan broj telefona
            if (string.IsNullOrWhiteSpace(txtTelefon.Text))
            {
                MessageBox.Show("Molimo unesite broj telefona!", 
                    "Greška validacije", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelefon.Focus();
                return;
            }

            // Izvlači selektovanu vrednost iz ComboBox-a
            string izabranaVrstaClanarine = (cmbTipClanarine.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Mesečna";
            
            // Računa datum isteka članarine na osnovu tipa
            DateTime datumIstekaClanske = IzracunajDatumIsteka(izabranaVrstaClanarine);

            // Kreira novi objekat člana sa unetim podacima
            var noviClan = new Clan(
                txtImePrezime.Text.Trim(),      // Uklanja suvišne razmake
                izabranaVrstaClanarine,
                txtTelefon.Text.Trim(),
                DateTime.Now,                   // Trenutni datum i vreme
                datumIstekaClanske
            );

            try
            {
                // Dodaje novog člana u listu
                listaClanova.Add(noviClan);

                // Formira string za upis u fajl
                string zapisZaFajl = $"{noviClan.ImePrezime}|{noviClan.TipClanarine}|{noviClan.Telefon}|" +
                                   $"{noviClan.DatumUpisa:yyyy-MM-dd HH:mm:ss}|{noviClan.DatumIsteka:yyyy-MM-dd}\n";
                
                // Dodaje zapis na kraj fajla
                File.AppendAllText(FAJL_SA_PODACIMA, zapisZaFajl);

                // Prikazuje poruku o uspešnoj registraciji
                MessageBox.Show($"Član {noviClan.ImePrezime} je uspešno registrovan!\n" +
                              $"Članarina važi do: {noviClan.DatumIsteka:dd.MM.yyyy}", 
                    "Uspešna registracija", MessageBoxButton.OK, MessageBoxImage.Information);

                // Čisti formu za novi unos
                OcistiFormu();

                // Osvežava tabelu sa članovima
                OsveziTabeluClanova();
            }
            catch (Exception ex)
            {
                // Prikazuje poruku o grešci ako upis nije uspeo
                MessageBox.Show($"Greška pri čuvanju člana: {ex.Message}", 
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda koja računa datum isteka članarine na osnovu tipa
        private DateTime IzracunajDatumIsteka(string tipClanarine)
        {
            // Koristi switch expression (C# 8.0) za elegantno grananje
            return tipClanarine switch
            {
                "Mesečna" => DateTime.Now.AddMonths(1),      // Dodaje 1 mesec
                "Kvartalna" => DateTime.Now.AddMonths(3),    // Dodaje 3 meseca
                "Polugodišnja" => DateTime.Now.AddMonths(6), // Dodaje 6 meseci
                "Godišnja" => DateTime.Now.AddYears(1),      // Dodaje 1 godinu
                "VIP" => DateTime.Now.AddYears(2),           // Dodaje 2 godine
                _ => DateTime.Now.AddMonths(1)               // Default je mesečna
            };
        }

        // Event handler za dugme "OBRIŠI FORMU"
        private void Obrisi_Click(object sender, RoutedEventArgs e)
        {
            OcistiFormu();
        }

        // Metoda koja briše sav sadržaj iz forme za unos
        private void OcistiFormu()
        {
            txtImePrezime.Clear();              // Briše tekst iz polja za ime
            txtTelefon.Clear();                 // Briše tekst iz polja za telefon
            cmbTipClanarine.SelectedIndex = 0;  // Vraća na prvu opciju
            txtImePrezime.Focus();              // Vraća fokus na prvo polje
        }
    }
}
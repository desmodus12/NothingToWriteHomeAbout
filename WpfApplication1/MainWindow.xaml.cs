using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using net.windward.api.csharp;
using WindwardInterfaces.net.windward.api.csharp;
using System.IO;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void SpravVysvedcenie(int komu)
        {
            // Winward engine
            Report.Init();

            FileStream template = File.OpenRead("../../testik.docx");
            FileStream output = File.Create("SQL Report.pdf");

            Report myReport = new ReportPdf(template, output);


            // string na SQL data source
            string strConn = "Data Source=SQL5004.Smarterasp.net;Initial Catalog=DB_9E4838_desmodus12;User Id=DB_9E4838_desmodus12_admin;Password=12stard9;";
            IReportDataSource data = new AdoDataSourceImpl("System.Data.SqlClient", strConn);

            myReport.ProcessSetup();

            Dictionary<string, object> map = new Dictionary<string, object>();
            //iba znamky konkretneho ziaka
            map.Add("ziak", komu);
            
            //This is the function where we actually tell our report the parameter values
            data.Map = map;

            //the second parameter is the name of the data source
            myReport.ProcessData(data, "MSSQL");
            myReport.ProcessComplete();

            //close out of our template file and output
            output.Close();
            template.Close();

            // Open the finished report
            string fullPath = System.IO.Path.GetFullPath("SQL Report.pdf");
            System.Diagnostics.Process.Start(fullPath);

        }


        private void ZozenZdatabazy(int id)
        {
            //vytvorime novy proces na pozadi, ktory sa pripoji na db a zozenie vysledok podla vstupneho parametra
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                SqlConnection conn = new SqlConnection("Data Source=SQL5004.Smarterasp.net;Initial Catalog=DB_9E4838_desmodus12;User Id=DB_9E4838_desmodus12_admin;Password=12stard9;");
                SqlDataAdapter da = new SqlDataAdapter("SELECT * from ziaci WHERE id=" + id, conn);
                // goo.gl/RTtnRY
                DataTable precopisetakedebilnenazvypremennych = new DataTable();
                int a = da.Fill(precopisetakedebilnenazvypremennych);

                if (a > 0)
                {
                    var ajaj = new string[] {
                    precopisetakedebilnenazvypremennych.Rows[0][0].ToString(),
                    precopisetakedebilnenazvypremennych.Rows[0][1].ToString(),
                    precopisetakedebilnenazvypremennych.Rows[0][2].ToString()
                    };
                    args.Result = ajaj;
                }
                else
                {
                    var ajaj = new string[] {
                    "Chyba",
                    "Epic fail!"
                    };
                    args.Result = ajaj;
                }

            });



            // BW skoncil, treba zistit ci uspesne, alebo nie
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    string[] nieco = (string[])args.Result;
                    if (nieco[0] == "Chyba")
                    {   //ziadny vysledok
                        MessageBox.Show("Sorry bratu, taketo id som nenasiel!" + nieco[0] + ": " + nieco[1]);
                    }
                    else
                    {  //uspesne, updateneme UI
                        textBox.Text = nieco[0];
                        textBox_Copy.Text = "Meno: " + nieco[1];
                        textBox_Copy2.Text = "Priezvisko: " + nieco[2];
                    }
                    image1.Visibility = Visibility.Hidden;
                    label1.Visibility = Visibility.Hidden;
                    button1.Visibility = Visibility.Visible;
                }
            });

            bw.RunWorkerAsync(id);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int id;
            // len preto, za sa iba hrame, nemuseli by sme nechat otvorene vratka pre sql injection
            bool parsed = Int32.TryParse(Vstup.Text, out id);

            if (!parsed)
            {  
                MessageBox.Show("Chyba : Toto nem riadne číslo!");
                // Preco cislo? V db su 4 zaznamy, sanca ze by uzivatel trafil akurat existujuce priezvisko je tak dost minimalna
            }
            else
            {
                ZozenZdatabazy(id);
                //info o posielani zajaca po pakety
                image1.Visibility = Visibility.Visible;
                label1.Visibility = Visibility.Visible;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int id;
            //Chceme "vysvedcenie" zobrazovaneho vysledku, nie vysledku v textboxe, ktory nemusi existovat
            bool parsed = Int32.TryParse(textBox.Text, out id);
            SpravVysvedcenie(id);
        }
    }
}

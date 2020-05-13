using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Npgsql;
using System.Configuration;

namespace GeoAreasUpdate
{
    public partial class FormMain : Form
    {
        private static string Host = ConfigurationManager.AppSettings["Host"];
        private static string User = ConfigurationManager.AppSettings["User"];
        private static string DBname = ConfigurationManager.AppSettings["DataBase"];
        private static string Password = ConfigurationManager.AppSettings["Psw"];
        private static string Port = ConfigurationManager.AppSettings["Port"];
        private static string Schema = ConfigurationManager.AppSettings["Schema"];
        private static string Table = ConfigurationManager.AppSettings["Table"];
        
        private String connString;

        private String filePath = string.Empty;

        public StreamWriter log;
        //public int progressivo = 200000;

        public FormMain()
        {
            InitializeComponent();
            if(Host is null || DBname is null || User is null || Password is null || Schema is null || Table is null || Port is null)
            {
                MessageBox.Show ("Una o più Key del file GeoAreaUpdate.exe.config non sono definite \n" + "Le key da definire sono :  Host,User,DBname,Port,Password,Schema,Table","ATTENZIONE");
                Environment.Exit(Environment.ExitCode);
            }
            connString =
               String.Format(
                   "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                   Host,
                   User,
                   DBname,
                   Port,
                   Password);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "zip files (*.zip)|*.zip";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    txtFileZip.Text = filePath;

                }
            }


        }

        //public void WorkThreadFunction()
        //{
        //    try
        //    {
        //        ZipFile.ExtractToDirectory(filePath, AppContext.BaseDirectory + "Tempfile");

        //    }
        //    catch (Exception ex)
        //    {
        //        // log errors
        //    }
        //}

        private void btnElabora_Click(object sender, EventArgs e)
        {
            // scompatto il file zip

            try
            {
                Directory.Delete(AppContext.BaseDirectory + "Tempfile", true);
            }
            catch (Exception ex)
            {

            }

            //Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            //thread.Start();

            Cursor = Cursors.WaitCursor;

            ZipFile.ExtractToDirectory(filePath, AppContext.BaseDirectory + "Tempfile");

            

            //return; 

            if (deletetype() )
            {
               
                string path = AppContext.BaseDirectory + "\\logFile.txt";
                try
                {
                    File.Delete(AppContext.BaseDirectory + "\\logFile.txt");
                }
                catch (Exception ex)
                {

                }
                log = File.CreateText(path);
                log.WriteLine("");
                log.WriteLine("Lista File kml non caricati");
                log.WriteLine("");
                string[] fileEntries = Directory.GetFiles(AppContext.BaseDirectory + "Tempfile", "*.kml");

                // ciclo su tutti i file
                progressBar1.Minimum = 0;
                progressBar1.Value = 0;
                progressBar1.Maximum = fileEntries.Count();
                foreach (string fileName in fileEntries)
                {
                    progressBar1.Value += 1;
                    ProcessFileKml(fileName);
                }

                log.Close();
            }

            Cursor = Cursors.Default;

            txtLog.Text = File.ReadAllText(AppContext.BaseDirectory + "\\logFile.txt");

            MessageBox.Show("Processo Terminato", "Information", MessageBoxButtons.OK);

        }

        public  void ProcessFileKml(string path)
        {
            // leggo il file xml/kml
            string filename = path;
            Update newElementGeo = new Update();


            XDocument doc =null;

            try
            {
                doc = XDocument.Load(path);
            }
            catch (Exception ex)
            {
                log.WriteLine(path + "File corrotto");
                return;
            }
            XElement root = doc.Root;
            XNamespace ns = root.GetDefaultNamespace();

            String descrDocument = null;
            var descrizione = doc.Element(ns + "kml").Element(ns + "Document").Element(ns + "name");
            descrDocument = descrizione.Value;

            var stili = doc.Element(ns + "kml").Element(ns + "Document").Elements(ns + "Style");



            var query = root
               .Element(ns + "Document")
               .Elements(ns + "Placemark")
               .Select(x => new Placemark
               {
                   Nome = x.Element(ns + "name").Value,
                   Descrizione = x.Element(ns + "description").Value,
                   ExtendedData = x.Element(ns + "ExtendedData").Elements(ns + "Data"),
                   Polygon = x.Descendants(ns + "Polygon"),
                   Style = x.Element(ns + "styleUrl")
                   // etc
               });

            foreach (Placemark place in query)
            {

                String CodiceComune = null;
                String CodiceZona = null;
                String CodiceDescrizione = null;
                String ListaCoordinate = null;
                String CodiceStyle = null;
                var codComune = from codici in place.ExtendedData
                                where codici.Attribute("name").Value == "CODCOM"
                                select codici;

                foreach (XElement el in codComune)
                {
                    CodiceComune = (String)el.Element(ns + "value");
                    CodiceDescrizione = "Comune:" + (String)el.Element(ns + "value");
                }


                var codZona = from codici in place.ExtendedData
                              where codici.Attribute("name").Value == "CODZONA"
                              select codici;

                foreach (XElement el in codZona)
                {
                    CodiceZona = (String)el.Element(ns + "value");
                    CodiceDescrizione += " Zona:" + (String)el.Element(ns + "value");

                }

                CodiceDescrizione += " " + descrDocument;

                foreach (var coord in place.Polygon.Descendants(ns + "outerBoundaryIs").Descendants(ns + "coordinates"))
                {
                    ListaCoordinate += coord.Value;
                }

                var aStyle = from st in stili
                             where st.Attribute("id").Value == place.Style.Value.Replace("#", "")
                             select st;
                foreach (XElement el in aStyle)
                {
                    CodiceStyle = el.Element(ns + "PolyStyle").Element(ns + "color").Value.Substring(2);

                }

                DateTime adesso = DateTime.Now;
                newElementGeo.Record_creation_time = adesso;
                newElementGeo.Record_update_time = adesso;
                newElementGeo.Code = CodiceComune + CodiceZona;
                newElementGeo.Description = CodiceDescrizione;
                
                String[] pezzi = ListaCoordinate.Replace("\n", "").Replace(",0 ", " ").TrimEnd().Split(' ');
                List<String> longitudine = new List<String>();
                List<String> latitudine= new List<String>();
                for (int i=0;i<pezzi.Count();i++)
                {
                    String[] appo = pezzi[i].Split(',');
                    longitudine.Add(appo[0]);
                    latitudine.Add(appo[1]);
                }


                newElementGeo.Point_number = pezzi.Count();
                newElementGeo.Polygon = ListaCoordinate.Replace("\n", "").Replace(",0 ", " ");
                newElementGeo.Last_update_time = adesso;
                newElementGeo.Color = "#" + CodiceStyle;
                newElementGeo.Longitude_min = longitudine.Min();
                newElementGeo.Longitude_max = longitudine.Max();
                newElementGeo.Latitude_min = latitudine.Min();
                newElementGeo.Latitude_max = latitudine.Max();

                if (!Inserttype(newElementGeo))
                {
                    log.WriteLine(path + "Nessun Inserimento per code :" + newElementGeo.Code);
                }
               
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                if (Schema =="")
                {
                    cmd.CommandText = "Select * from " + Table + " limit 100";
                }
                else
                {
                    cmd.CommandText = "Select * from " + "\"" + Schema + "\"." + Table + " limit 100";
                }
                cmd.CommandType = CommandType.Text;
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmd.Dispose();
                conn.Close();
                bindingSource1.DataSource = dt;
                dataGridView1.DataSource = bindingSource1;
            }
        }
        private bool deletetype()
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                if (Schema == "")
                {
                    cmd.CommandText = "delete  from " + Table + " Where type_id=1";
                }
                else
                {
                    cmd.CommandText = "delete  from " + "\"" + Schema + "\"." + Table + " Where type_id=1";
                }
                cmd.CommandType = CommandType.Text;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Query eliminazione Type1 non riuscita ", "Delete not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }
                
                cmd.Dispose();
                conn.Close();

                return true;
            }
        }
        public  bool Inserttype(Update list)
        {

            
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;

                // nota che @id andrà tolto perche sarà AUTOINCREMENT
                if (Schema == "")
                {
                    cmd.CommandText = "Insert Into " + Table + "(record_creation_time,record_update_time,code,description,type_id,latitude_min,longitude_min,latitude_max,longitude_max,point_number,polygon,last_update_time,external_code,color) Values(@record_creation_time,@record_update_time,@code,@description,@type_id,@latitude_min,@longitude_min,@latitude_max,@longitude_max,@point_number,@polygon,@last_update_time,@external_code,@color)";
                }
                else
                {
                    cmd.CommandText = "Insert Into " + "\"" + Schema + "\"." + Table + "(record_creation_time,record_update_time,code,description,type_id,latitude_min,longitude_min,latitude_max,longitude_max,point_number,polygon,last_update_time,external_code,color) Values(@record_creation_time,@record_update_time,@code,@description,@type_id,@latitude_min,@longitude_min,@latitude_max,@longitude_max,@point_number,@polygon,@last_update_time,@external_code,@color)";
                }
                cmd.CommandType = CommandType.Text;
                //cmd.Parameters.Add(new NpgsqlParameter("@id", progressivo++));
                cmd.Parameters.Add(new NpgsqlParameter("@record_creation_time", list.Record_creation_time));
                cmd.Parameters.Add(new NpgsqlParameter("@record_update_time", list.Record_update_time));
                cmd.Parameters.Add(new NpgsqlParameter("@code", list.Code));
                cmd.Parameters.Add(new NpgsqlParameter("@description", list.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@type_id", list.Type_id));
                cmd.Parameters.Add(new NpgsqlParameter("@latitude_min", Convert.ToDouble(list.Latitude_min.Replace(".",","))));
                cmd.Parameters.Add(new NpgsqlParameter("@longitude_min", Convert.ToDouble(list.Longitude_min.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@latitude_max", Convert.ToDouble(list.Latitude_max.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@longitude_max", Convert.ToDouble(list.Longitude_max.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@point_number", list.Point_number));
                cmd.Parameters.Add(new NpgsqlParameter("@polygon", list.Polygon));
                cmd.Parameters.Add(new NpgsqlParameter("@last_update_time", list.Last_update_time));
                cmd.Parameters.Add(new NpgsqlParameter("@external_code", list.External_code));
                cmd.Parameters.Add(new NpgsqlParameter("@color", list.Color));

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Query inserimento dati Type1 non riuscita ", "Insert not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }

                cmd.Dispose();
                conn.Close();
                return true;
            }
        }
    }
}

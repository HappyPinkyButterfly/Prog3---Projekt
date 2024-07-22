using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Pozresna_kacica
{
    public partial class Form1 : Form
    {

        private Timer timer = new Timer();
        private List<Point> kaca = new List<Point>();
        private Point sadez = Point.Empty;
        private int trenutneTocke = 0;
        private int velikostCelice = 20;
        private string smer = "right";
        private List<(string ime, int tocke)> tocke = new List<(string, int)>();
        private Panel ploscaZaTocke;
        private Panel ploscaZaNastavitve;
        private Label TabRezLabel;
        private Color barva = Color.Green;
        private bool zacetekPremikanja = false;
        private Label trenutneTockeLabel;
        private Button start;
        private Button pomoc;
        private Button teren;
        private bool terenVklopljen = false;
        private string uporabniskoIme;
        private string povNiz = @"DataSource=.\TOCKE.sqlite;Version=3;";
        private ComboBox gorComboBox;
        private ComboBox dolComboBox;
        private ComboBox levoComboBox;
        private ComboBox desnoComboBox;
        private List<Rectangle> ovire = new List<Rectangle>();
        private Label steviloIgerLabel;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
            Baza();
            Prijava();
        }
        private void InitializeGame()
        {
            this.ClientSize = new Size(1000, 600);
            this.Text = "Požrešna kačica";
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // nemoremo povečati okna z vlečenjem
            this.MaximizeBox = false;                           // nemoremo povečati okna z gumbom za pocečanje okna

            timer.Interval = 100;
            timer.Tick += Timer;
            timer.Start();

            this.Paint += Risanje;
            this.KeyDown += Form1_KeyDown;

            ploscaZaTocke = new Panel();
            ploscaZaTocke.Location = new Point(0, 0);
            ploscaZaTocke.Size = new Size(200, 600);
            ploscaZaTocke.BorderStyle = BorderStyle.FixedSingle;

            trenutneTockeLabel = new Label();
            trenutneTockeLabel.Location = new Point(10, 90); // Prilagodi lokacijo glede na steviloIgerLabel
            trenutneTockeLabel.Size = new Size(180, 30);
            trenutneTockeLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            trenutneTockeLabel.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaTocke.Controls.Add(trenutneTockeLabel);

            steviloIgerLabel = new Label();
            steviloIgerLabel.Location = new Point(10, 50); // Nastavi ustrezno lokacijo
            steviloIgerLabel.Size = new Size(180, 30);
            steviloIgerLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            steviloIgerLabel.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaTocke.Controls.Add(steviloIgerLabel);

            TabRezLabel = new Label();
            TabRezLabel.Location = new Point(10, 10);
            TabRezLabel.Size = new Size(180, 480);
            TabRezLabel.Font = new Font("Arial", 10);
            TabRezLabel.TextAlign = ContentAlignment.MiddleCenter;
            TabRezLabel.Padding = new Padding(0, 120, 0, 0);
            ploscaZaTocke.Controls.Add(TabRezLabel);

            this.Controls.Add(ploscaZaTocke);

            ploscaZaNastavitve = new Panel();
            ploscaZaNastavitve.Location = new Point(800, 0);
            ploscaZaNastavitve.Size = new Size(200, 600);
            ploscaZaNastavitve.BorderStyle = BorderStyle.FixedSingle;

            start = new Button();
            start.Text = "START";
            start.Size = new Size(150, 30);
            start.Location = new Point(25, 50);
            start.Click += START_click;

            pomoc = new Button();
            pomoc.Text = "Pomoc - Off";
            pomoc.Size = new Size(150, 30);
            pomoc.Location = new Point(25, 150);
            pomoc.Click += pomoc_click;

            teren = new Button();
            teren.Text = "Teren - Off";
            teren.Size = new Size(150, 30);
            teren.Location = new Point(25, 250);
            teren.Click += teren_click;

            Label gor = new Label();
            gor.Text = "Sprememba tipke \"Gor\"";
            gor.Location = new Point(25, 300);
            gor.Size = new Size(150, 20);
            gor.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaNastavitve.Controls.Add(gor);

            gorComboBox = new ComboBox();
            gorComboBox.Location = new Point(25, 320);
            gorComboBox.Size = new Size(150, 30);
            gorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;// padajoč seznam, kjer uporabnik ne sme sam vpisat
            gorComboBox.Items.AddRange(new string[] { "Up", "Down", "Left", "Right" }.Concat(Enumerable.Range('A', 26).Select(i => ((char)i).ToString())).ToArray());

            Label dol = new Label();
            dol.Text = "Sprememba tipke \"Dol\"";
            dol.Location = new Point(25, 360);
            dol.Size = new Size(150, 20);
            dol.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaNastavitve.Controls.Add(dol);

            dolComboBox = new ComboBox();
            dolComboBox.Location = new Point(25, 380);
            dolComboBox.Size = new Size(150, 30);
            dolComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dolComboBox.Items.AddRange(new string[] { "Up", "Down", "Left", "Right" }.Concat(Enumerable.Range('A', 26).Select(i => ((char)i).ToString())).ToArray());

            Label levo = new Label();
            levo.Text = "Sprememba tipke \"Levo\"";
            levo.Location = new Point(25, 420);
            levo.Size = new Size(150, 20);
            levo.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaNastavitve.Controls.Add(levo);

            levoComboBox = new ComboBox();
            levoComboBox.Location = new Point(25, 440);
            levoComboBox.Size = new Size(150, 30);
            levoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            levoComboBox.Items.AddRange(new string[] { "Up", "Down", "Left", "Right" }.Concat(Enumerable.Range('A', 26).Select(i => ((char)i).ToString())).ToArray());



            Label desno = new Label();
            desno.Text = "Sprememba tipke \"Desno\"";
            desno.Location = new Point(25, 480);
            desno.Size = new Size(150, 20);
            desno.TextAlign = ContentAlignment.MiddleCenter;
            ploscaZaNastavitve.Controls.Add(desno);

            desnoComboBox = new ComboBox();
            desnoComboBox.Location = new Point(25, 500);
            desnoComboBox.Size = new Size(150, 30);
            desnoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            desnoComboBox.Items.AddRange(new string[] { "Up", "Down", "Left", "Right" }.Concat(Enumerable.Range('A', 26).Select(i => ((char)i).ToString())).ToArray());

            // to nam naredi, da imamo že vnaprej napisan kaj ima combo box izbran
            gorComboBox.SelectedItem = "Up";
            dolComboBox.SelectedItem = "Down";
            levoComboBox.SelectedItem = "Left";
            desnoComboBox.SelectedItem = "Right";

            ploscaZaNastavitve.Controls.Add(start);
            ploscaZaNastavitve.Controls.Add(pomoc);
            ploscaZaNastavitve.Controls.Add(teren);
            ploscaZaNastavitve.Controls.Add(gorComboBox);
            ploscaZaNastavitve.Controls.Add(dolComboBox);
            ploscaZaNastavitve.Controls.Add(levoComboBox);
            ploscaZaNastavitve.Controls.Add(desnoComboBox);

            this.Controls.Add(ploscaZaNastavitve);

            tocke = NaloziTocke();
            NovaIgra();
            PosodobiTabeloTock();
        }

        private void Prijava()
        {
            using (Form prijavniObrazec = new Form())
            {
                prijavniObrazec.StartPosition = FormStartPosition.CenterParent;
                prijavniObrazec.Size = new Size(300, 200);
                prijavniObrazec.FormBorderStyle = FormBorderStyle.FixedDialog;
                prijavniObrazec.MaximizeBox = false;
                prijavniObrazec.MinimizeBox = false;
                prijavniObrazec.ControlBox = false;

                Label prijavaNapis = new Label();
                prijavaNapis.Text = "Vnesite uporabniško ime:";
                prijavaNapis.Location = new Point(10, 20);
                prijavaNapis.Size = new Size(260, 30);
                prijavaNapis.TextAlign = ContentAlignment.MiddleCenter;
                prijavniObrazec.Controls.Add(prijavaNapis);

                TextBox prijavaTextBox = new TextBox();
                prijavaTextBox.Location = new Point(10, 60);
                prijavaTextBox.Size = new Size(260, 30);
                prijavniObrazec.Controls.Add(prijavaTextBox);

                Button prijavniGumb = new Button();
                prijavniGumb.Text = "Prijava";
                prijavniGumb.Location = new Point(100, 100);
                prijavniGumb.Click += (sender, e) => {
                    if (string.IsNullOrWhiteSpace(prijavaTextBox.Text))
                    {
                        MessageBox.Show("Prosimo, vnesite veljavno uporabniško ime.", "Napaka", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        uporabniskoIme = prijavaTextBox.Text;             // funkcija gumba
                        prijavniObrazec.DialogResult = DialogResult.OK;
                        prijavniObrazec.Close();
                    }
                };
                prijavniObrazec.Controls.Add(prijavniGumb);

                prijavniObrazec.ShowDialog(); // ne moremo komunicirati z glavno aplikacijo, dokler ne zapremo tega okna.
            }
        }



        private void Timer(object sender, EventArgs e)
        {
            if (zacetekPremikanja)   // radi bi da se premikanje zacne sele ob pritisku gumba, vec o temu nizje
            {
                Premikanje();
                PreveriTrk();
                Invalidate();
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void START_click(object sender, EventArgs e)
        {
            if (PreveriEnakeKontrole())
            {
                MessageBox.Show("Imate nepravilno nastavljene kontrole. Prosimo, da popravite nastavitve.", "Napaka", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            start.Enabled = false;                 // izklopimo vse na desni plosci, kar nam dovoli uspešno igranje igre, vendar se vse vklopi nazaj ob koncu igre
            pomoc.Enabled = false;
            teren.Enabled = false;
            gorComboBox.Enabled = false;
            dolComboBox.Enabled = false;
            levoComboBox.Enabled = false;
            desnoComboBox.Enabled = false;
        }
        private bool PreveriEnakeKontrole()
        {
            string[] tab = { gorComboBox.SelectedItem.ToString(), dolComboBox.SelectedItem.ToString(), levoComboBox.SelectedItem.ToString(), desnoComboBox.SelectedItem.ToString() };
            for (int i = 0; i < tab.Length; i++)
            {
                for (int j = i + 1; j < tab.Length; j++)
                {
                    if (tab[i] == tab[j])
                    {
                        return true;  // torej ce smo nasli enako kontrole bomo sli v notrajnost if stavka in bomo dobili napako
                    }
                }
            }
            return false;

        }

        private void pomoc_click(object sender, EventArgs e)
        {
            if (pomoc.Text == "Pomoc - Off")
            {
                pomoc.Text = "Pomoc - On";
            }
            else
            {
                pomoc.Text = "Pomoc - Off";
            }

        }

        private void teren_click(object sender, EventArgs e)
        {
            if (teren.Text == "Teren - Off")
            {
                teren.Text = "Teren - On";
                terenVklopljen = true;

                // Ustvari ovire, če teren še ni bil vklopljen
                if (ovire.Count == 0) // preveri ce je seznam ovir prazen
                {
                    UstvariOvire();
                }
            }
            else
            {
                teren.Text = "Teren - Off";
                terenVklopljen = false;
            }
            this.Invalidate();
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void NovaIgra()
        {
            // kača vedno bo zacela igro na sredini
            int centerX = this.Width / (2 * velikostCelice);
            int centerY = this.Height / (2 * velikostCelice);

            kaca.Clear();

            kaca.Add(new Point(centerX, centerY));
            kaca.Add(new Point(centerX - 1, centerY));
            kaca.Add(new Point(centerX - 2, centerY));

            // nastavimo smer kače na desno, da ko bomo kateri koli
            // gumb kliknili na tipkovnci se bo ta začela premikati desno, tako kot je usmernjena
            smer = "right";
            zacetekPremikanja = false;

            trenutneTocke = 0;
            trenutneTockeLabel.Text = "Trenutne točke: " + trenutneTocke;
            UstvariSadez();
            PosodobiTabeloTock();
            PovecajZaEnoIgraneIgre();
        }

        private void KonecIgre()
        {
            timer.Stop();
            ShraniDosezeneTocke(uporabniskoIme, trenutneTocke);
            tocke = NaloziTocke(); // Osveži seznam rezultatov
            int pozicija = tocke.FindIndex(t => t.ime == uporabniskoIme && t.tocke == trenutneTocke) + 1;

            if (pozicija <= 20 && (trenutneTocke != 0))
            {
                MessageBox.Show($"Čestitamo! Uvrstili ste se med 20 najboljših igralcev vseh časov! Vaše točke: {trenutneTocke}");
            }
            else if ((pozicija <= 100) && (trenutneTocke != 0))
            {
                MessageBox.Show($"Izjemno! Uvrstili ste se med top 100 igralcev! Vaš rezultat: {trenutneTocke}");

            }
            else
            {
                MessageBox.Show($"Vaš rezultat je: {trenutneTocke}. Čeprav se tokrat niste uvrstili med najvišje, ste se odlično potrudili! Poskusite znova in premaknite svoje meje!");

            }

            this.KeyDown -= Form1_KeyDown;

            DialogResult result = MessageBox.Show("Želite igrati znova?", "Konec igre", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)  // če izberemo yes, nam spet dovoli, da si nastavimo svoje nastavitve
            {
                NovaIgra();
                this.KeyDown += Form1_KeyDown;
                timer.Start();
                start.Enabled = true;
                pomoc.Enabled = true;
                teren.Enabled = true;
                gorComboBox.Enabled = true;
                dolComboBox.Enabled = true;
                levoComboBox.Enabled = true;
                desnoComboBox.Enabled = true;
            }
            else
            {
                Application.Exit(); // Zapre igro
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void UstvariSadez()
        {
            Random rnd = new Random();
            Rectangle kvadratSadeza;
            Rectangle igralnoPolje = new Rectangle(ploscaZaTocke.Width, 0, this.ClientSize.Width - ploscaZaTocke.Width - ploscaZaNastavitve.Width, this.ClientSize.Height);

            bool veljavnaPozicija = false;

            while (!veljavnaPozicija)
            {
                // poskusimo nov sadez ustvariti
                sadez = new Point(rnd.Next(ploscaZaTocke.Width / velikostCelice, (this.ClientSize.Width - ploscaZaNastavitve.Width) / velikostCelice), rnd.Next(0, this.ClientSize.Height / velikostCelice));


                kvadratSadeza = new Rectangle(sadez.X * velikostCelice, sadez.Y * velikostCelice, velikostCelice, velikostCelice);

                // preverimo, da je sadez na veljavni poziciji
                veljavnaPozicija = igralnoPolje.Contains(kvadratSadeza) && !Sadez_v_kaci() && (!terenVklopljen || !SadezNaOviro(kvadratSadeza));
            }
        }
        // to sem naredil, da se sadez ne zgenerira znotraj ovire
        private bool SadezNaOviro(Rectangle kvadratSadeza)
        {
            foreach (Rectangle ovira in ovire)
            {
                if (kvadratSadeza.IntersectsWith(ovira))
                {
                    return true; // ce je to true, je sadez na oviri, kar nocemo in s tem pravilno delamo zgoraj ko preverjamo
                }
            }
            return false;
        }


        // to sem naredil, da se sadez ne zgenerira znotraj kace
        private bool Sadez_v_kaci()
        {
            Rectangle kvadratSadeza = new Rectangle(sadez.X * velikostCelice, sadez.Y * velikostCelice, velikostCelice, velikostCelice);

            foreach (Point kosKace in kaca)
            {
                Rectangle kvadratKosKace = new Rectangle(kosKace.X * velikostCelice, kosKace.Y * velikostCelice, velikostCelice, velikostCelice);
                if (kvadratSadeza.IntersectsWith(kvadratKosKace))
                {
                    return true;
                }
            }
            return false;
        }
        private void UstvariOvire()
        {
            Random rnd = new Random();
            Rectangle igralnoPolje = new Rectangle(ploscaZaTocke.Width, 0, this.ClientSize.Width - ploscaZaTocke.Width - ploscaZaNastavitve.Width, this.ClientSize.Height);

            int[] velikosti = { 2, 2, 5, 3, 2, 4 }; // velikosti ovir, torej prva je 2x2, druga 5x3 in tretji tip ovire je 2x4
            int[] kolicinaOvire = { 10, 5, 3 }; // število ovir različnih tipov

            for (int i = 0; i < kolicinaOvire.Length; i++)  // lahko razumemo tako, da i predstavlja posamezno oviro, npr i = 0 predstalja 10 ovir oblike 2x2
            {
                int širina = velikosti[2 * i];
                int višina = velikosti[2 * i + 1];

                for (int j = 0; j < kolicinaOvire[i]; j++)
                {

                    Rectangle ovira = new Rectangle();
                    bool oviraJeOvirana = true;

                    while (oviraJeOvirana)
                    {
                        Point Lokacija = new Point(rnd.Next(ploscaZaTocke.Width / velikostCelice, (this.ClientSize.Width - ploscaZaNastavitve.Width - širina * velikostCelice) / velikostCelice), rnd.Next(0, (this.ClientSize.Height - višina * velikostCelice) / velikostCelice));
                        ovira = new Rectangle(Lokacija.X * velikostCelice, Lokacija.Y * velikostCelice, širina * velikostCelice, višina * velikostCelice);
                        oviraJeOvirana = Oviran(ovira); // !igralnoPolje.Contains(ovira) ||
                    }

                    ovire.Add(ovira);
                }
            }
        }



        private bool Oviran(Rectangle Ovira)
        {
            foreach (Rectangle ovira in ovire)  // tukaj si zagotovimo, da so vse ovire vsaj 2 celice narazen me sabo, da med genreacijo ovir ne pride do česa čudnega
            {
                Rectangle PogojnaOvira = new Rectangle(ovira.X - 2 * velikostCelice, ovira.Y - 2 * velikostCelice, ovira.Width + 4 * velikostCelice, ovira.Height + 4 * velikostCelice);
                if (PogojnaOvira.IntersectsWith(Ovira))
                {
                    return true;
                }
            }

            foreach (Point kosKace in kaca)  // preverimo da se ovira ne pojavi na kaci
            {
                Rectangle kvadratKosKace = new Rectangle(kosKace.X * velikostCelice, kosKace.Y * velikostCelice, velikostCelice, velikostCelice);
                if (kvadratKosKace.IntersectsWith(Ovira))
                {
                    return true;
                }
            }

            Rectangle sadezKvadrat = new Rectangle(sadez.X * velikostCelice, sadez.Y * velikostCelice, velikostCelice, velikostCelice);
            if (sadezKvadrat.IntersectsWith(Ovira)) // enako se za sadez
            {
                return true;
            }

            return false;
        }
        private void Risanje(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            for (int i = 0; i < kaca.Count; i++)
            {
                if (i == 0)
                {
                    g.FillRectangle(Brushes.Brown, new Rectangle(kaca[i].X * velikostCelice, kaca[i].Y * velikostCelice, velikostCelice, velikostCelice));
                }
                else
                {
                    g.FillRectangle(new SolidBrush(barva), new Rectangle(kaca[i].X * velikostCelice, kaca[i].Y * velikostCelice, velikostCelice, velikostCelice));
                }
            }

            g.FillRectangle(Brushes.Red, new Rectangle(sadez.X * velikostCelice, sadez.Y * velikostCelice, velikostCelice, velikostCelice));

            if (terenVklopljen)  // ko prizgemo teren, narisemo ovire
            {
                foreach (var ovira in ovire)
                {
                    g.FillRectangle(Brushes.Black, ovira);
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void PreveriTrk()
        {
            Point glava = kaca[0];

            // preverimo trk z robom
            if (glava.X < 0 || glava.Y < 0 || glava.X >= this.ClientSize.Width / velikostCelice || glava.Y >= this.ClientSize.Height / velikostCelice)
            {
                KonecIgre();
                return;  // imamo return, da ko je konec igre da metoda ne preverja naprej za druge opcije
            }

            // preverimo trk same s sabo
            for (int i = 1; i < kaca.Count; i++)
            {
                if (glava == kaca[i])
                {
                    KonecIgre();
                    return;
                }
            }

            // preverimo trk s ploščami
            if (PreveriTrkPlosc(glava))
            {
                KonecIgre();
                return;
            }

            // preverimo trk z ovirami, če je teren vklopljen
            if (terenVklopljen)
            {
                Rectangle kvadratGlave = new Rectangle(glava.X * velikostCelice, glava.Y * velikostCelice, velikostCelice, velikostCelice);
                foreach (var ovira in ovire)
                {
                    if (kvadratGlave.IntersectsWith(ovira))
                    {
                        KonecIgre();
                        return;
                    }
                }
            }
        }
        private bool PreveriTrkPlosc(Point head)
        {
            Rectangle kvadratGlave = new Rectangle(head.X * velikostCelice, head.Y * velikostCelice, velikostCelice, velikostCelice);
            Rectangle kvPloToc = new Rectangle(ploscaZaTocke.Location, ploscaZaTocke.Size);
            Rectangle kvPloNas = new Rectangle(ploscaZaNastavitve.Location, ploscaZaNastavitve.Size);

            if (pomoc.Text == "Pomoc - On")
            {
                // preverimo trk s ploščo glede na trenutno smer
                if (smer == "left" && head.X < 0)
                {
                    return false; //  prehod skozi levi rob dovoljen, če je pomoč vklopljena
                }
                else if (smer == "right" && head.X >= (this.ClientSize.Width - ploscaZaNastavitve.Width) / velikostCelice)
                {
                    return false; //  prehod skozi desni rob dovoljen , če je pomoč vklopljena
                }
                else
                {
                    // preverimo trk s ploščami, če pomoč ni vklopljena
                    return kvPloToc.Contains(kvadratGlave) || kvPloNas.Contains(kvadratGlave);
                }
            }
            else
            {
                // preverimo trk s ploščami, če pomoč ni vklopljena
                return kvPloToc.Contains(kvadratGlave) || kvPloNas.Contains(kvadratGlave);
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            string Gor = gorComboBox.SelectedItem.ToString();
            string Dol = dolComboBox.SelectedItem.ToString();
            string Levo = levoComboBox.SelectedItem.ToString();
            string Desno = desnoComboBox.SelectedItem.ToString();

            bool premik = false;
            string pressedKey = e.KeyCode.ToString();

            if (pressedKey.Equals(Gor, StringComparison.OrdinalIgnoreCase))  // dodatni pogoj bo samo dvovljil, da equals nebo občutljiv na velikost znakov, majhne ali velike črke
            {
                if (smer != "down") smer = "up";  //imamo te dodatne pogoje, da če hočemo iti gor, vedar gremo ap dol se to nemore zgoditi, saj bi kačica potem umrla

                premik = true;
            }
            else if (pressedKey.Equals(Dol, StringComparison.OrdinalIgnoreCase))
            {
                if (smer != "up") smer = "down";
                premik = true;
            }
            else if (pressedKey.Equals(Levo, StringComparison.OrdinalIgnoreCase))
            {
                if (smer != "right") smer = "left";
                premik = true;
            }
            else if (pressedKey.Equals(Desno, StringComparison.OrdinalIgnoreCase))
            {
                if (smer != "left") smer = "right";
                premik = true;
            }

            if (premik)
            {
                zacetekPremikanja = true;
            }
        }
        private void Premikanje()
        {
            Point glava = kaca[0];
            Point novaGlava = glava;

            
            if (smer == "up")
            {
                novaGlava.Y -= 1;
                if (novaGlava.Y < 0) // prehod skozi zgornji rob
                {
                    if (pomoc.Text == "Pomoc - On")
                    {
                        novaGlava.Y = (this.ClientSize.Height / velikostCelice) - 1; // premaknemo kacico na spodni rob in se od tam premika naprej
                    }
                    else
                    {
                        KonecIgre(); // igra se konča, če ni pomoči
                        return;
                    }
                }
            }
            else if (smer == "down")
            {
                novaGlava.Y += 1;
                if (novaGlava.Y >= this.ClientSize.Height / velikostCelice) // prehod skozi spodnji rob
                {
                    if (pomoc.Text == "Pomoc - On")
                    {
                        novaGlava.Y = 0;
                    }
                    else
                    {
                        KonecIgre(); 
                        return;
                    }
                }
            }
            else if (smer == "left")
            {
                novaGlava.X -= 1;
                if (novaGlava.X < ploscaZaTocke.Width / velikostCelice) // prehod skozi levi rob
                {
                    if (pomoc.Text == "Pomoc - On")
                    {
                        novaGlava.X = (this.ClientSize.Width - ploscaZaNastavitve.Width) / velikostCelice - 1;
                    }
                    else
                    {
                        KonecIgre(); 
                        return;
                    }
                }
            }
            else if (smer == "right")
            {
                novaGlava.X += 1;
                if (novaGlava.X >= (this.ClientSize.Width - ploscaZaNastavitve.Width) / velikostCelice) // prehod skozi desni rob
                {
                    if (pomoc.Text == "Pomoc - On")
                    {
                        novaGlava.X = ploscaZaTocke.Width / velikostCelice;
                    }
                    else
                    {
                        KonecIgre(); 
                        return;
                    }
                }
            }

            // simulacija premikanje kace
            kaca.Insert(0, novaGlava);

            if (novaGlava == sadez)
            {
                trenutneTocke += 10;
                trenutneTockeLabel.Text = "Trenutne točke: " + trenutneTocke;  //ker tukaj ne odstranimo zadnjega dela kace insamo dodamo kaco, tukaj simuliramo povecanje kace
                UstvariSadez();
            }
            else
            {
                kaca.RemoveAt(kaca.Count - 1);
            }
        }


        private void PosodobiTabeloTock()
        {
            string tockeText = "Najboljših 20:\n\n";
            for (int i = 0; i < 20; i++)
            {
                tockeText += $"{i + 1}. {tocke[i].ime}: {tocke[i].tocke}\n";
            }
            TabRezLabel.Text = tockeText;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Baza()
        {
            using (SQLiteConnection povezava1 = new SQLiteConnection(povNiz)) // raje uporabil using da si zagotovim da se povezava2 zapre
            {
                povezava1.Open();


                // tabela za sledenje tock uporabnikov in za glavno prikazno tabelo top 20 igralcev
                string TabTockSQL = @"
                    CREATE TABLE IF NOT EXISTS TOCKE (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Score INTEGER NOT NULL
                    )";
                SQLiteCommand ukaz1 = new SQLiteCommand(TabTockSQL, povezava1);
                ukaz1.ExecuteNonQuery();

                // s bazo si bomo pomagal slediti stevilo vseh igranih iger 
                string stIgranihIger = @"
                    CREATE TABLE IF NOT EXISTS COUNTER (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GameCount INTEGER NOT NULL
                    )";
                SQLiteCommand ukaz2 = new SQLiteCommand(stIgranihIger, povezava1);
                ukaz2.ExecuteNonQuery();

                // vstavi zacetno vrednost v tabelo za stetje iger
                string insertInitialCounterQuery = @"  
                    INSERT INTO COUNTER (GameCount)
                    SELECT 0 WHERE NOT EXISTS (SELECT 1 FROM COUNTER)"; 
                SQLiteCommand ukaz3 = new SQLiteCommand(insertInitialCounterQuery, povezava1);
                ukaz3.ExecuteNonQuery();
            }
        }
        private void ShraniDosezeneTocke(string uporabniskoIme, int dosezeneTocke)
        {
            using (SQLiteConnection povezava2 = new SQLiteConnection(povNiz))
            {
                povezava2.Open();
                string popravljenoUporabniskoIme = uporabniskoIme.Replace("'", ""); //preprecimo napake kjer bi uporabniska imena vsebovala enojne narekovaje
                string vstavi = $"INSERT INTO TOCKE (Username, Score) VALUES ('{popravljenoUporabniskoIme}', {dosezeneTocke})";
                SQLiteCommand ukaz4 = new SQLiteCommand(vstavi, povezava2);
                ukaz4.ExecuteNonQuery();
            }
        }

        private List<(string ime, int tocke)> NaloziTocke()
        {
            List<(string ime, int tocke)> tocke = new List<(string ime, int tocke)>();

            using (SQLiteConnection povezava = new SQLiteConnection(povNiz))
            {
                povezava.Open();
                string izvedba = "SELECT Username, Score FROM TOCKE ORDER BY Score DESC LIMIT 100";
                SQLiteCommand uakz5 = new SQLiteCommand(izvedba, povezava);
                using (SQLiteDataReader branje = uakz5.ExecuteReader())
                {
                    while (branje.Read())
                    {
                        string uporabniskoIme = branje.GetString(0);
                        int dosezeneTocke = branje.GetInt32(1);
                        tocke.Add((uporabniskoIme, dosezeneTocke));
                    }
                }
            }

            return tocke;
        }
        private void PovecajZaEnoIgraneIgre()
        {
            using (SQLiteConnection povezava = new SQLiteConnection(povNiz))
            {
                povezava.Open();
                string izvedba = "SELECT GameCount FROM COUNTER";
                SQLiteCommand ukaz6 = new SQLiteCommand(izvedba, povezava);
                steviloIgerLabel.Text = $"Število igranja: {Convert.ToInt32(ukaz6.ExecuteScalar())}";

                string updateQuery = "UPDATE COUNTER SET GameCount = GameCount + 1";  
                SQLiteCommand uakz7 = new SQLiteCommand(updateQuery, povezava);
                uakz7.ExecuteNonQuery();   
            } 
        }
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
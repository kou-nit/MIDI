using NextMidi.Data;
using NextMidi.Data.Domain;
using NextMidi.Data.Score;
using NextMidi.DataElement;
using NextMidi.Filing.Midi;
using NextMidi.MidiPort.Output;
using NextMidi.Time;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace NextMidiTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        MyMidiOutPort MyMidiOutPort;
        MidiData MidiData;
        MidiPlayer Player;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Player.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ポートの指定
            MyMidiOutPort = new MyMidiOutPort(new MidiOutPort(0));
            Console.WriteLine(MyMidiOutPort.IsOpen);

            // 指定したポートを開く
            try
            {
                MyMidiOutPort.Open();
            }
            catch
            {
                Console.WriteLine("no such port exists");
                return;
            }
            // ファイルパスの指定
            //            string path = "test.mid";
            string path = "test.mid";
            if (!File.Exists(path))
            {
                Console.WriteLine("File dose not exist");
                return;
            }
            // midiファイルの読み込み
            MidiData = MidiReader.ReadFrom("test.mid", Encoding.GetEncoding("shift-jis"));
            MidiFileDomain domain = new MidiFileDomain(MidiData);
            // Playerの作成
            Player = new MidiPlayer(MyMidiOutPort);
            Player.Stopped += Player_Stopped;
            // 別スレッドでの演奏開始
            Player.Play(domain);
        }


        void Player_Stopped(object sender, EventArgs e)
        {
            MyMidiOutPort.Close();
        }

        private void Slider_Value_VelocityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyMidiOutPort != null)
            {
                MyMidiOutPort.deltaVelocity = (int)e.NewValue;
            }
        }

        private void Slider_Value_TempoChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyMidiOutPort != null)
            {
                Console.WriteLine("{0} -> {1}", MyMidiOutPort.Coef, e.NewValue);
                MyMidiOutPort.Coef = e.NewValue;
            }
        }
    }
}

/// <summary>
/// MidiOutPortのSend改良版
/// </summary>
class MyMidiOutPort : IMidiOutPort
{
    MidiOutPort Delegate;
    /// <summary>
    /// Tickの比率
    /// </summary>
    public double Coef = 1.0;
    /// <summary>
    /// Velocityの増減量
    /// </summary>
    public int deltaVelocity = 0;
    /// <summary>
    /// Velocityの最大
    /// </summary>
    private const byte MaxVelocity = 127;
    /// <summary>
    /// MyMidiOutPort のインスタンス
    /// </summary>
    /// <param name="index"></param>
    public MyMidiOutPort(MidiOutPort MidiOutPort)
    {
        Delegate = MidiOutPort;
    }
    /// <summary>
    /// MidiOutPortのIsOpen
    /// </summary>
    public bool IsOpen
    {
        get
        {
            return Delegate.IsOpen;
        }
        set
        {
            Delegate.IsOpen = value;
        }
    }
    /// <summary>
    /// MidiOutPortのName
    /// </summary>
    public string Name
    {
        get
        {
            return Delegate.Name;
        }
    }
    /// <summary>
    /// MidiOutPortのClose()
    /// </summary>
    public void Close()
    {
        Delegate.Close();
    }
    /// <summary>
    /// MidiOutPortのOpen()
    /// </summary>
    public void Open()
    {
        Delegate.Open();
    }
    /// <summary>
    /// dataを加工し, MidiOutPortのSendを使う
    /// </summary>
    /// <param name="data"></param>
    public void Send(IMidiEvent data)
    {
        //ここでデータ加工
        if (data.RequireToSend)
        {
            modifyData(data);
        }
        Delegate.Send(data);
    }

    private void modifyData(IMidiEvent data)
    {
        if (data is NoteOnEvent)
        {
            // Velocity, Tickの変更
            var Note = (NoteOnEvent)data;
            Note.Velocity = (int)(Note.Velocity) + deltaVelocity > 0 ? (byte)Math.Min(127, (int)(Note.Velocity) + deltaVelocity) : (byte)0;
            Note.Tick = (int)(Note.Tick * Coef);
            Console.WriteLine("{0}", Coef);
        }
    }
}
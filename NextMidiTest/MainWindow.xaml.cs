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
            string path = "test.mid";
            if (!File.Exists(path))
            {
                Console.WriteLine("File dose not exist");
                return;
            }
            // midiファイルの読み込み
            MidiData = MidiReader.ReadFrom("test.mid");
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
    }
}

/// <summary>
/// MidiOutPortのSend改良版
/// </summary>
class MyMidiOutPort : IMidiOutPort
{
    MidiOutPort Delegate;
    private int Coef = 1;
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
        modifyData(data);
        Delegate.Send(data);
    }

    private void modifyData(IMidiEvent data)
    {
        if (data is NoteOnEvent)
        {
            // Velocity, Tickの変更
            var Note = (NoteOnEvent)data;
            Note.Velocity = 0;
        }
    }
}
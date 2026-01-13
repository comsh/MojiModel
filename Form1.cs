using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using static System.StringComparison;

namespace MojiModel {
public partial class Form1:Form {
    public Form1() {
        InitializeComponent();
        InitBrowser();
    }
    async void InitBrowser(){
        var opt=new CoreWebView2EnvironmentOptions("--allow-file-access-from-files --disable-background-networking --disable-component-update --disable-sync --no-proxy-server --disable-webrtc");
        var env= await CoreWebView2Environment.CreateAsync(null,null,opt);
        await webview2.EnsureCoreWebView2Async(env);
        string site=Directory.GetCurrentDirectory().Replace("\\","/");
        webview2.CoreWebView2.Navigate($"file://{site}/index.html");
        webview2.WebMessageReceived+=OnMessageReceived;
        webview2.NavigationCompleted+=OnPageReady;
    }
    private void OnPageReady(object from,CoreWebView2NavigationCompletedEventArgs args){
        var path=Directory.GetCurrentDirectory()+@"\js\font\";
        string[] files=Directory.GetFiles(path);
        if(files==null) return;
        string fonts="";
        for(int i=0; i<files.Length; i++) if(files[i].EndsWith(".json",Ordinal)){
            fonts+="\n"+Path.GetFileNameWithoutExtension(files[i]);
        }
        webview2.CoreWebView2.PostWebMessageAsString(fonts);
    }

    private void OnMessageReceived(object from,CoreWebView2WebMessageReceivedEventArgs args){
        try{
            string txt=args.TryGetWebMessageAsString();
            var mf=ModelConvert.OBJ2Model(txt);
            string fn=OutFileDialog();
            if(fn!="") mf.Write(fn);
        }catch(Exception e){
            MessageBox.Show(e.StackTrace, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return;
    }

    private string lastOutFile="";
    public string OutFileDialog() {
        SaveFileDialog dialog = new SaveFileDialog();
        if(lastOutFile!="") dialog.InitialDirectory=Path.GetDirectoryName(lastOutFile);
        dialog.Title = "出力ファイル選択";
        dialog.Filter = "modelファイル|*.model";
        string fn="";
        if (dialog.ShowDialog() == DialogResult.OK) fn=dialog.FileName;
        dialog.Dispose();
        if(fn!="") lastOutFile=fn;
        return fn;
    }
}
}

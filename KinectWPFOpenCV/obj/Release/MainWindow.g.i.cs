﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F31EB3657FFD2559ED144FDD1A92F1A4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace KinectWPFOpenCV {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock title;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtError;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtOut;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewbox outputViewbox;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image outImg;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioColor;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image colorImg;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioDepth;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image depthImg;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtBlobCount;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtOscStatus;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtInfo;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtFPS;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioBG;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image bgImg;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAutoCapture;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnManualCapture;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnClear;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioDiff;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image diffImg;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioTrack;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image trackImg;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkFlipH;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkFlipV;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkAutoMin;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderMin;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkAutoMax;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderMax;
        
        #line default
        #line hidden
        
        
        #line 120 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderMinSize;
        
        #line default
        #line hidden
        
        
        #line 126 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderMaxSize;
        
        #line default
        #line hidden
        
        
        #line 140 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderThreshold;
        
        #line default
        #line hidden
        
        
        #line 146 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkOsc;
        
        #line default
        #line hidden
        
        
        #line 149 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtOscIP;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtOscPort;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/KinectWPFOpenCV;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 20 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CloseBtnClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.title = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.txtError = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.txtOut = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.outputViewbox = ((System.Windows.Controls.Viewbox)(target));
            return;
            case 6:
            this.outImg = ((System.Windows.Controls.Image)(target));
            return;
            case 7:
            this.radioColor = ((System.Windows.Controls.RadioButton)(target));
            
            #line 37 "..\..\MainWindow.xaml"
            this.radioColor.Checked += new System.Windows.RoutedEventHandler(this.radioColor_Checked);
            
            #line default
            #line hidden
            return;
            case 8:
            this.colorImg = ((System.Windows.Controls.Image)(target));
            
            #line 41 "..\..\MainWindow.xaml"
            this.colorImg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.colorImg_MouseDown);
            
            #line default
            #line hidden
            return;
            case 9:
            this.radioDepth = ((System.Windows.Controls.RadioButton)(target));
            
            #line 45 "..\..\MainWindow.xaml"
            this.radioDepth.Checked += new System.Windows.RoutedEventHandler(this.radioDepth_Checked);
            
            #line default
            #line hidden
            return;
            case 10:
            this.depthImg = ((System.Windows.Controls.Image)(target));
            
            #line 49 "..\..\MainWindow.xaml"
            this.depthImg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.depthImg_MouseDown);
            
            #line default
            #line hidden
            return;
            case 11:
            this.txtBlobCount = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 12:
            this.txtOscStatus = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 13:
            this.txtInfo = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 14:
            this.txtFPS = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 15:
            this.radioBG = ((System.Windows.Controls.RadioButton)(target));
            
            #line 69 "..\..\MainWindow.xaml"
            this.radioBG.Checked += new System.Windows.RoutedEventHandler(this.radioBG_Checked);
            
            #line default
            #line hidden
            return;
            case 16:
            this.bgImg = ((System.Windows.Controls.Image)(target));
            
            #line 73 "..\..\MainWindow.xaml"
            this.bgImg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.bgImg_MouseDown);
            
            #line default
            #line hidden
            return;
            case 17:
            this.btnAutoCapture = ((System.Windows.Controls.Button)(target));
            
            #line 75 "..\..\MainWindow.xaml"
            this.btnAutoCapture.Click += new System.Windows.RoutedEventHandler(this.btnAutoCapture_Click);
            
            #line default
            #line hidden
            return;
            case 18:
            this.btnManualCapture = ((System.Windows.Controls.Button)(target));
            
            #line 76 "..\..\MainWindow.xaml"
            this.btnManualCapture.Click += new System.Windows.RoutedEventHandler(this.btnManualCapture_Click);
            
            #line default
            #line hidden
            return;
            case 19:
            this.btnClear = ((System.Windows.Controls.Button)(target));
            
            #line 77 "..\..\MainWindow.xaml"
            this.btnClear.Click += new System.Windows.RoutedEventHandler(this.btnClear_Click);
            
            #line default
            #line hidden
            return;
            case 20:
            this.radioDiff = ((System.Windows.Controls.RadioButton)(target));
            
            #line 80 "..\..\MainWindow.xaml"
            this.radioDiff.Checked += new System.Windows.RoutedEventHandler(this.radioDiff_Checked);
            
            #line default
            #line hidden
            return;
            case 21:
            this.diffImg = ((System.Windows.Controls.Image)(target));
            
            #line 84 "..\..\MainWindow.xaml"
            this.diffImg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.diffImg_MouseDown);
            
            #line default
            #line hidden
            return;
            case 22:
            this.radioTrack = ((System.Windows.Controls.RadioButton)(target));
            
            #line 86 "..\..\MainWindow.xaml"
            this.radioTrack.Checked += new System.Windows.RoutedEventHandler(this.radioTrack_Checked);
            
            #line default
            #line hidden
            return;
            case 23:
            this.trackImg = ((System.Windows.Controls.Image)(target));
            
            #line 90 "..\..\MainWindow.xaml"
            this.trackImg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.trackImg_MouseDown);
            
            #line default
            #line hidden
            return;
            case 24:
            this.chkFlipH = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 25:
            this.chkFlipV = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 26:
            this.chkAutoMin = ((System.Windows.Controls.CheckBox)(target));
            
            #line 102 "..\..\MainWindow.xaml"
            this.chkAutoMin.Checked += new System.Windows.RoutedEventHandler(this.chkAutoMin_Checked);
            
            #line default
            #line hidden
            
            #line 102 "..\..\MainWindow.xaml"
            this.chkAutoMin.Unchecked += new System.Windows.RoutedEventHandler(this.chkAutoMin_Unchecked);
            
            #line default
            #line hidden
            return;
            case 27:
            this.sliderMin = ((System.Windows.Controls.Slider)(target));
            return;
            case 28:
            this.chkAutoMax = ((System.Windows.Controls.CheckBox)(target));
            
            #line 111 "..\..\MainWindow.xaml"
            this.chkAutoMax.Checked += new System.Windows.RoutedEventHandler(this.chkAutoMax_Checked);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.chkAutoMax.Unchecked += new System.Windows.RoutedEventHandler(this.chkAutoMax_Unchecked);
            
            #line default
            #line hidden
            return;
            case 29:
            this.sliderMax = ((System.Windows.Controls.Slider)(target));
            return;
            case 30:
            this.sliderMinSize = ((System.Windows.Controls.Slider)(target));
            return;
            case 31:
            this.sliderMaxSize = ((System.Windows.Controls.Slider)(target));
            return;
            case 32:
            this.sliderThreshold = ((System.Windows.Controls.Slider)(target));
            return;
            case 33:
            this.chkOsc = ((System.Windows.Controls.CheckBox)(target));
            
            #line 146 "..\..\MainWindow.xaml"
            this.chkOsc.Checked += new System.Windows.RoutedEventHandler(this.chkOsc_Checked);
            
            #line default
            #line hidden
            
            #line 146 "..\..\MainWindow.xaml"
            this.chkOsc.Unchecked += new System.Windows.RoutedEventHandler(this.chkOsc_Unchecked);
            
            #line default
            #line hidden
            return;
            case 34:
            this.txtOscIP = ((System.Windows.Controls.TextBox)(target));
            
            #line 149 "..\..\MainWindow.xaml"
            this.txtOscIP.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.txtOscIP_TextChanged);
            
            #line default
            #line hidden
            return;
            case 35:
            this.txtOscPort = ((System.Windows.Controls.TextBox)(target));
            
            #line 151 "..\..\MainWindow.xaml"
            this.txtOscPort.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.txtOscPort_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


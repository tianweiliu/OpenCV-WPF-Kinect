   M S B A M L   `   `   `  ����   KinectWPFOpenCV     KinectWPFOpenCV.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$"  Kinect OpenCV blob detection��$
��1024��$	��768�� � WindowStyle$
 None=� � AllowsTransparency .  � Icon$ Images/Status.png��.���� ���#00bcf2$��1q�._�� $��0,0,-1,0q� � RowDefinitions �� $��90 ��� $��* � � ColumnDefinitions �� �� $	��35* ��� $	��165 ��� $
��auto �.+��� -      SimpleButton#��[  $��2��$��Right=�$��2��$	��Top=�$��0 10 q�.���� $��X��  SegoeSymbol#��[ $��12(����#999�� $��Images\Status.png��$
��None=�$
��Left=�$��1���� -    ��Name$ title��$��Kinect + OpenCV ��$
��Left=�$��
65,26,0,26q�$��Center=�  KinectFontLight#��[   TitleForegroundBrush#��[   HeaderFontSize#��[ $��1���� ���#666$��1��$��
15 0 10 15q�$��2���� -   $ txtError��$��1��$��No Kinect Found��$y�	Collapsed=�#��[ ���#ccc$��21(�$��Center=�$��Center=�$��2���� $��
15 0 10 15q�$��1��$��2��.+��� -   $��txtOut��$��Tracking�� �� Padding$ 10q����#ccc$��0��(� -    (�Name$ outputViewbox��	 (� Stretch$	 Uniform=�.���� -   $��outImg��$	��640��$	��480��
 J� RenderTransformOrigin$
 0.5,0.5&���  �� Orientation$ 
Horizontal=�$��0 0 0 0q�.+��� .+��� $ 
Horizontal=����#ccc$��42��.+�	� -   $��
radioColor��$��Center=�$��	10 11 0 0q�.���� $��Color��$��	0 -14 0 0q�$ 10q��� -   $��colorImg��$��0,0,0,0q�$	��160��$	��120��$��Uniform=�$
 0.5,0.5&�$��Right=�$	��Top=��� .+��� $ 
Horizontal=����#ccc$��42��.+�	� -	   $��
radioDepth��$��Center=�$��	10 11 0 0q�.���� $��Depth��$��	0 -14 0 0q�$ 10q��� -
   $��depthImg��$��0,0,0,0q�$	��160��$	��120��$��Uniform=�$
 0.5,0.5&�$��Right=�$	��Top=��� .+��� ���#ccc$
��Info��$��0,0,0,0q�$ 10q�$��42��$	��198���� $ 
Horizontal=�$��0 10 0 0q�.+��� $��Blobs Detected��$��10 0 0 0q����	#FFC7C7C7�� -   $ txtBlobCount��$
��Wrap=�$��0��$��10 0 0 0q����	#FFC7C7C7�� -   $ txtOscStatus��$
��Wrap=�$��
Osc Status��$��	10 10 0 0q����	#FFC7C7C7�� $ 
Horizontal=�$��0 10 0 0q�.+��� $��Kinect��$��10 0 0 0q����	#FFC7C7C7�� -   $ txtInfo��$
��Wrap=�$	��N/A��$��10 0 0 0q����	#FFC7C7C7�� -   $ txtFPS��$
��Wrap=�$��0��$��10 0 0 0q����	#FFC7C7C7�� $��0 0 0 15q�$	��160�����#f0f0f0$
��Left=�$��2��$��1��.+��� $ 
Horizontal=����#ccc$��42��.+�	� -   $��radioBG��$��Center=�$��	10 11 0 0q�.���� $��
Background��$��	0 -14 0 0q�$ 10q��� -   $��bgImg��$��0,0,0,0q�$	��160��$	��120��$��Uniform=�$
 0.5,0.5&�$��Right=�$	��Top=��� $ 
Horizontal=�.+��� -   $��btnAutoCapture��$��5 5 5 5q�$��45��$��30��$
��Auto���� -   $��btnManualCapture��$��0 5 5 5q�$��55��$��30��$��Manual���� -   $��btnClear��$��0 5 5 5q�$��40��$��30��$��Clear���� $ 
Horizontal=����#ccc$��42��.+�	� -   $��	radioDiff��$��Center=�$��	10 11 0 0q�.���� $��Differencing��$��	0 -14 0 0q�$ 10q��� -   $��diffImg��$��0,0,0,0q�$	��160��$	��120��$��Uniform=�$
 0.5,0.5&�$��Right=�$	��Top=��� $ 
Horizontal=����#ccc$��42��.+�	� -   $��
radioTrack��$��Center=�$��	10 11 0 0q� d� 	IsChecked$
 TrueY�.���� $��Tracking��$��	0 -14 0 0q�$ 10q��� -   $��trackImg��$��0,0,0,0q�$	��160��$	��120��$��Uniform=�$
 0.5,0.5&�$��Right=�$	��Top=��� $��	0 0 15 15q�$	��300�����#f0f0f0$��Right=�$��3��$��1��.+��� $��Settings��$ 10q����#ccc�� $��Image��$ 10q��� $ 
Horizontal=�.+��� -    ��Name$ chkFlipH��$��Flip Horizontal��$��Center=�$��10q�$
 TrueY��� -   $ chkFlipV��$��Flip Vertical��$��Center=�$��10q��� $ 
Horizontal=�.+��� $��Min Distance��$ 10q��� -   $ 
chkAutoMin��$
��Auto��$��Center=�$
 TrueY��� $ 
Horizontal=�.+��� -    ��Name$ 	sliderMin��$	��250�� � Minimum$ 0Q� � Value$ 0Q� � Maximum$
 3000Q� �� TickFrequency$ 1Q� �� IsSnapToTickEnabled .  �� Interval$ 1��$��5q�$��10q�{�.  �� $��Center=����� *Value+ �� ElementName$ 	sliderMin���� $ 
Horizontal=�.+��� $��Max Distance��$ 10q��� -   $ 
chkAutoMax��$
��Auto��$��Center=�$
 TrueY��� $ 
Horizontal=�.+��� -   $ 	sliderMax��$	��250��$	 900Q�$
 6000Q�$
 6000Q�$ 1Q� . $ 1��$��5q�$��10q�{�.  �� $��Center=����� *Value+$ 	sliderMax���� $��Min Blob Size��$ 10q��� $ 
Horizontal=�.+��� -   $ sliderMinSize��$	��250��$ 1Q�$ 25Q�$ 50Q�$��5q�$ 1Q� . $ 1��$��10q��� $��Center=����� *Value+$ sliderMinSize���� $��Max Blob Size��$ 10q��� $ 
Horizontal=�.+��� -   $ sliderMaxSize��$	��250��$ 1Q�$	 555Q�$	 555Q�$��5q�$ 1Q� . $ 1��$��10q��� $��Center=����� *Value+$ sliderMaxSize���� $��	Threshold��$ 10q��� $ 
Horizontal=�$��0 0 0 10q�.+��� -    $ sliderThreshold��$	��250��$ 0Q�$ 10Q�$	 255Q�$��5q�$ 1Q� . $ 1��$��10q��� $��Center=����� *Value+$ sliderThreshold���� $ 
Horizontal=����#ccc$��42��.+��� $��Open Sound Control��$ 10q����#ccc�� -!   $ chkOsc��$��Connect��$��Center=�$
 TrueY��� $��
IP Address��$ 10q��� -"    ��Name$ txtOscIP��$��	10 0 10 0q�$��	127.0.0.1���� $
��Port��$ 10q��� -#   $ 
txtOscPort��$��	10 0 10 0q�$��12000��
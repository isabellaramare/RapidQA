   M S B A M L   `   `   `  ���� C  ?ImSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=,clr-namespace:ImSystem.Log;assembly=ImSystemImSystem.Log  ? ;ImUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA,clr-namespace:ImUI.ColorPicker;assembly=ImUIImUI.ColorPicker 3%clr-namespace:ImUI.Zoom;assembly=ImUI	ImUI.Zoom  RapidQA   RapidQA.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    >d2http://schemas.microsoft.com/expression/blend/2008    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     <log,clr-namespace:ImSystem.Log;assembly=ImSystem     Dcolorpicker,clr-namespace:ImUI.ColorPicker;assembly=ImUI     6zoom%clr-namespace:ImUI.Zoom;assembly=ImUI       � Title$  Rapid QA��$	��700��$
��1200��$	��500��$	��100�� � 	Resources    NoHoverButton �� 
TargetType �� Property �� Value �� 
TargetType �� Triggers P� Property P� Value��%B  &	        '��     �� " ��.���  �@�OverridesDefaultStyle . ��  �@�Cursor$
 Hand����  � �� �� " ��.R��� $��border��$��0q�
���   �#��z��._��� $
��Left=�$��Center=� P�  �@J�IsMouseOver . . ���  �@J�Opacity$	 0.5Q��� " ��.���  � ��$ Stretch=�.��� -   	 �Name$	 MainGrid��
 � ColumnDefinitions
 �� $
��1.1* �$��5��$	��400���� $��3 ��� $��4* �.+�� -   $	 
GridImages��$��2�� � RowDefinitions �� $��30 ��� $��3* ��� $��3 ��� $��20 �$��20��.+�� $��0�����	#FF777777
 �� �� .+��� $��0�� �� Orientation$ 
Horizontal=�$
��Left=�$��Center=�.+��� -    ��Name$ BtnInfo��$��i��$��Right=�$��Center=�$��20��$��5q����	#FF4EAD4E$
��Bold"�$��Italic$�
��������� $��Work Area Size:��$��Center=�$��11(��� -    ��Name$ workarea_width��$��Center=�$��0��$��70��$	��5,0q��� $��x��$��Center=��� -   $ workarea_height��$��Center=�$��0��$��70��$	��5,0q��� $��Background color:��$��Center=�$��20,0,0,0q�"  ImUI.ColorPicker.ColorPicker  -   $��ClrPcker_Background��$��1��$��Center=�$
��10,0q��� $��1��$ 
Horizontal=�$��Right=�.+��� -   $ BtnCopyView��$��	Copy View��$��20��$��Center=�$��70��$��11(�$��
0, 0, 2, 0q��� -   $ BtnSaveView��$��	Save View��$��20��$��Center=�$��70��$��11(�$��
0, 0, 7, 0q��� -	   $ BtnCopyImage��$��
Copy Image��$��20��$��Center=�$��70��$��11(�$��
0, 0, 2, 0q��� -
   $ BtnSaveImage��$��
Save Image��$��20��$��Center=�$��70��$��11(�$��
0, 0, 5, 0q�� -   $	 ViewGrid��$��1�� �� $	��29* ��� $
��186* �.+�  ImUI.Zoom.ZoomControl  -     Name$
 Zoom�� J� 	AllowDrop . ���	#FF4B4B4B$	��Top=�$��2��.��� -   $	 Workarea��.+��� -    ��Name$ 	ZoomImage�� �� BitmapScalingMode$ NearestNeighbor=�$	��Top=�$
��Left=�$
��None=� J� SnapsToDevicePixels .  ����  �� ElementName$ 	ImageZoom�� �� Path$ Source��� -   $ 	BtnExpand��$	��[ ]��$��Right=�$	��Top=�$��20�����	#FF4EAD4E$��5q�$��0,0,0,3q�$
��Bold"�
��������� -    ��Name$ border2�����	#CC343434$��0q�$��30��$
��Left=�$��	15,0,0,12q�$��Bottom=�$	��214�� �� CornerRadius$ 10�����	#FF343434$��1��._�� $	��200��
 �� $	��76* ��� $
��106* �.+��� -    ��Name$ imageColorRectangle��$��16�����	#FF343434$��16��$��0��
���   �$��0,7,0,0q�$	��Top=�$
��Left=��� -    ��Name$ imageRedLabel��$��0��$��9(�
���  ��$��22��$
��Left=�$��49,4,0,0q�$	��Top=�$��37��$��2���� -   $ imageGreenLabel��$��0��$��9(�
��� � �$��22��$��81,4,0,0q�$	��Top=�$
��Left=�$��37��$��2���� -   $ imageBlueLabel��$��0��$��9(����	#FF0094FF$��22��$
��Left=�$��30,4,0,0q�$	��Top=�$��35��$��1���� -   $ imageLuminanceLabel��$��0��$��9(�
�������$��22��$
��Left=�$��77,4,0,0q�$	��Top=�$��38��$��1���� -   $ label29��$��L:��$��10(�
�������$��22��$
��Left=�$��63,3,0,0q�$	��Top=�$��21��$
��Bold"�$��1���� -   $ label30��$��sRGB:��$��10(�
�������$��22��$��15,3,0,0q�$	��Top=�$��Normal$�$
��Bold"�$
��Left=�$��40���� $��Stretch=�$��Stretch=�$��2�����	#FF202020� $��3��.+��� -    ��Name$ CkbToggleLog��$��
Toggle Log��$��75��$��Bottom=�$��10(�$��Stretch=�$
��left=�$��0��$��0,2,0,2q�   ImSystem.Log.LogTextBox  -     Name$ 	LogWindow���� -   $ 
BorderInfo��$ 10�����	#CA343434$��Right=�$	��230��$��0,15,-248,200q�$��2��$y�	Collapsed=� K� ZIndex$ 2��._��� -    ��Name$ TxtInfo�� �� Padding$ 12q�
�������$
��Wrap=� �� 
LineHeight$ 20��� -   $	 GrdMenu��$��0��$��1�����	#FF777777 �� $��30 ��� $��25 ��� �� $��90 �.+��� $��0��$ Vertical=�.+��� $��0q�$��Quality Assurance��$��0�����	#FF202020$��30��$��Center=�$
��10,0q�$��18(�$
��Bold"�
�������� $��1��$	��Top=�$��	10,0,10,0q�
 �� $��1* ��� $
��2.5* ��� $��1* ��� $
��3.5* ��� $��1* ��� $��1* ��� $��1* �.+�� ���	#FF777777�� ���	#FF777777$��0,0,0,-2q�$��1��$��10(�$
��NAME��
��������� ���	#FF777777$��0,0,0,-2q�$��2��$��10(�$��2��$��IMAGE��
�������� $��4�����	#FF777777.+��� $��	0,-4,0,-4q�$��Icons/Visibility.png��$��20��� $��5�����	#FF777777.+��� $��	0,-4,0,-4q�$��Icons/Lock.png��$��20��� $��-0.5,0,-0.5,0q�$��6�����	#FF777777.+��� $��	0,-4,0,-4q�$��Icons/Delete.png��$��20��� $��2��.+��� -     ��Name$  scrollViewer��.���� 
�������$��10, 0, 10, 10q�.+��� -   ! ��Name$! sp�� . � $��0,5,0,0q�
 �� $��1* ��� $	��10* �.+��� -   $ BtnAddLayer��$��1��$��25��$��10,0,0,0q�#��[  $��Add Layer +��$��Stretch=���Z� � $��3��$��0,7,0,15q�
 �� �� .+��� $��Center=�$��0��$��10,0,0,0q�.+��� -    $ Ckb_AllVisible��$��0��$��	_Show all���� -!   $ BtnDeleteAll��$��0��$��_Delete All��$��Center=�$��0,5,0,0q�$��80��$	��120���� -"   $ BtnSavePreset��$��0��$��Save Preset��$��Center=�$��0,5,0,0q�$��80��$	��120���� $��Center=�$��1��$��0,0,10,0q�.+��� -#   $ Ckb_AllLocked��$��1��$��	_Lock all���� -$   $ BtnResetAll��$��1��$��
_Reset All��$��Center=�$��0,5,0,0q�$��80��$	��120���� -%   $ BtnLoadPreset��$��2��$��Load Preset��$��Center=�$��0,5,0,0q�$��80��$	��120���� -&   " ��Name$" Splitter��$��Stretch=�$��Stretch=�$��1��$��2�����	#FF202020
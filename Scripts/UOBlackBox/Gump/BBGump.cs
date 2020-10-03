//UO Black Box - By GoldDraco13

using Server.Gumps;
using Server.Network;

namespace Server.UOBlackBox
{
    class BBGump : Gump
    {
        public string _Type { get; set; }
        public int _ID1 { get; set; }
        public int _ID2 { get; set; }
        public int _W { get; set; }
        public int _H { get; set; }
        public int _Hue { get; set; }
        public string _Text { get; set; }

        public BBGump(int _ID1 = 0, int _ID2 = 0, int _W = 0, int _H = 0, string _Text = "", int _Hue = 0, string _Type = "", int x = 0, int y = 0) : base(0, 0)
        {
            int _X = x;
            int _Y = y;

            Disposable = false;
            Closable = false;
            Resizable = false;
            Dragable = false;

            AddPage(0);

            if (_Type == "Background")
                AddBackground(_X, _Y, _W, _H, _ID1);

            if (_Type == "Alpha Region")
                AddAlphaRegion(_X, _Y, _W, _H);

            if (_Type == "Image")
                AddImage(_X, _Y, _ID1);
            if (_Type == "Image Tiled")
                AddImageTiled(_X, _Y, _W, _H, _ID1);
            if (_Type == "Label")
                AddLabel(_X, _Y, _Hue, _Text);
            if (_Type == "Label Cropped")
                AddLabelCropped(_X, _Y, _W, _H, _Hue, _Text);

            if (_Type == "TextEntry")
                AddTextEntry(_X, _Y, _W, _H, _Hue, 0, _Text, _Text.Length);
            if (_Type == "Html")
                AddHtml(_X, _Y, _W, _H, _Text, false, false);

            if (_Type == "Item")
                AddItem(_X, _Y, _ID1, _Hue);
            if (_Type == "Button")
                AddButton(_X, _Y, _ID1, (_ID1++), 0, GumpButtonType.Reply, 0); //Alt : GumpButtonType.Page

            if (_Type == "Radio")
                AddRadio(_X, _Y, _ID1, (_ID1++), true, 0);
            if (_Type == "Check")
                AddCheck(_X, _Y, _ID1, (_ID1++), true, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            //PlayerMobile pm = sender.Mobile as PlayerMobile;

            //switch (info.ButtonID)
            //{
                //case 0:
                    //{
                        //pm.SendMessage("Case : 0");
                        //break;
                    //}
                //default:
                    //{
                        //pm.SendMessage("Case : Default");
                        //break;
                    //}
            //}
        }
    }
}

﻿//Apache2, 2014-present, WinterDev
using LayoutFarm.CustomWidgets;
namespace LayoutFarm
{
    [DemoNote("1.11_1 MultipleLabels")]
    class Demo_MultipleLabels : App
    {
        protected override void OnStart(AppHost host)
        {
            //PixelFarm.Drawing.RequestFont font = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 20);
            PixelFarm.Drawing.RequestFont font = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 20);
            for (int i = 0; i < 10; ++i)
            {
                Label label = new Label();
                label.SetLocation(i * 20, i * 20);
                label.TextColor = PixelFarm.Drawing.Color.Black;
                label.SetFont(font);
                label.Text = "Lpppyf ABCDEFG HI0123 456789 ABD";
                host.AddChild(label);
            }
        }
    }


    [DemoNote("1.11_2 MultipleLabels2")]
    class Demo_MultipleLabels2 : App
    {
        protected override void OnStart(AppHost host)
        {
            for (int i = 0; i < 10; ++i)
            {
                TextFlowLabel label = new TextFlowLabel(100, 50);
                label.SetLocation(i * 55, i * 55);
                //label.Color = PixelFarm.Drawing.Color.Black;
                label.Text = "ABCDEFG\r\nHIJKLMNOP\r\nQRSTUVWXZYZ\r\n0123456789";
                host.AddChild(label);
            }
        }
    }
}
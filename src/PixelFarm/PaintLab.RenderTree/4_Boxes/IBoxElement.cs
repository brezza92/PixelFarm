﻿//Apache2, 2014-2018, WinterDev

namespace LayoutFarm.UI
{
    public interface IBoxElement
    {
        void ChangeElementSize(int w, int h);
        int MinHeight { get; }
    }
   

}
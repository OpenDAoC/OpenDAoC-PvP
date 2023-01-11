using System;
using System.Collections.Generic;
using DOL.GS;

namespace DOL.GS;

public class Bindstones
{
    private List<BindstoneLocation> AvailableBindstones;

    public Bindstones()
    {
        AvailableBindstones = new List<BindstoneLocation>();
        //albion
        AvailableBindstones.Add(new BindstoneLocation(1, 560633, 511913, 2280)); //cotswold
        AvailableBindstones.Add(new BindstoneLocation(1, 585883, 476699, 2600)); //castle sauvage
        AvailableBindstones.Add(new BindstoneLocation(1, 531411, 479331, 2200)); //ludlow
        AvailableBindstones.Add(new BindstoneLocation(1, 573973, 530022, 2896)); //prydwen keep
        AvailableBindstones.Add(new BindstoneLocation(1, 585331, 531675, 2072)); //prydwen bridge
        AvailableBindstones.Add(new BindstoneLocation(1, 500252, 590006, 1829)); //camp station
        AvailableBindstones.Add(new BindstoneLocation(1, 470440, 630586, 1712)); //adribard retreat

        //midgard
        AvailableBindstones.Add(new BindstoneLocation(100, 804732, 724037, 4680)); //mularn
        AvailableBindstones.Add(new BindstoneLocation(100, 804660, 701402, 4960)); //haggerfel
        AvailableBindstones.Add(new BindstoneLocation(100, 765247, 668363, 5736)); //svasud
        AvailableBindstones.Add(new BindstoneLocation(100, 774718, 755221, 4600)); //vasudheim
        AvailableBindstones.Add(new BindstoneLocation(100, 724935, 760014, 4528)); //audliten
        AvailableBindstones.Add(new BindstoneLocation(100, 712204, 784099, 4672)); //huginfel
        AvailableBindstones.Add(new BindstoneLocation(100, 749257, 816004, 4408)); //ft atla
        AvailableBindstones.Add(new BindstoneLocation(100, 798949, 893340, 4744)); //galplen
        
        //hibernia
        AvailableBindstones.Add(new BindstoneLocation(200, 345972, 490734, 5200)); //mag mell
        AvailableBindstones.Add(new BindstoneLocation(200, 339590, 467280, 5200)); //ardee
        AvailableBindstones.Add(new BindstoneLocation(200, 333303, 420565, 5184)); //druimligen
        AvailableBindstones.Add(new BindstoneLocation(200, 344730, 528336, 5448)); //tir na mbeo
        AvailableBindstones.Add(new BindstoneLocation(200, 351916, 554260, 5106)); //ardagh
        AvailableBindstones.Add(new BindstoneLocation(200, 343364, 591653, 5456)); //howth
        AvailableBindstones.Add(new BindstoneLocation(200, 296117, 642170, 4848)); //connla
        AvailableBindstones.Add(new BindstoneLocation(200, 335039, 720014, 4296)); //innis carthaig
        
        //TODO - add SI zone release options
    }

    public BindstoneLocation GetRandomBindstone()
    {
        int index = Util.Random(AvailableBindstones.Count - 1);
        return AvailableBindstones[index];
    }
}

public class BindstoneLocation
{
    public int Region;
    public int X;
    public int Y;
    public int Z;

    public BindstoneLocation(int region, int x, int y, int z)
    {
        Region = region;
        X = x;
        Y = y;
        Z = z;
    }
}

public static class BindstoneManager
{
    public static Bindstones BindstoneList;

    static BindstoneManager()
    {
        BindstoneList = new Bindstones();
    }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Collections.Generic;

using VMusage;

using System.Threading;

class RecvBroadcst:IDisposable
{
    // event signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public RecvBroadcst()
    {
        //dataStream = new byte[1024];
        //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //myThread = new Thread(startReceive);
        //myThread.Start();
        StartReceive();
    }

    void listener_onUpdate(object sender, VMusage.procVMinfo data)
    {
        updateStatus(data);
    }
    public void Dispose(){
        if (receiveSocket != null){
            receiveSocket.Close();
            receiveSocket = null;
        }
    }

    //#######################################################################################

    Socket receiveSocket;
    byte[] recBuffer;
    EndPoint bindEndPoint;
    const int maxBuffer = 32768;

    public bool StopReceive()
    {
        bool bRet = false;
        try
        {
            //receiveSocket.Disconnect(false);
            receiveSocket.Close();
            receiveSocket = null;
            bRet = true;
        }
        catch(SocketException ex) {
            System.Diagnostics.Debug.WriteLine("StopReceive(): SocketException=" + ex.Message);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("StopReceive(): Exception=" + ex.Message); }
        return bRet;
    }

    public void StartReceive()
    {
        try
        {
            if (receiveSocket != null)
                StopReceive();
            receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bindEndPoint = new IPEndPoint(IPAddress.Any, 3002);

            recBuffer = new byte[maxBuffer];
            receiveSocket.Bind(bindEndPoint);
            receiveSocket.BeginReceiveFrom(recBuffer, 0, recBuffer.Length, SocketFlags.None, ref bindEndPoint, new AsyncCallback(MessageReceivedCallback), (object)this);

        }
        catch (SocketException ex)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("StartReceive SocketException: {0} ", ex.Message));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("StartReceive Exception: {0} ", ex.Message));
        }
    }

    void MessageReceivedCallback(IAsyncResult result)
    {
        EndPoint remoteEndPoint = new IPEndPoint(0, 0);

        //IPEndPoint LocalIPEndPoint = new IPEndPoint(IPAddress.Any, 3001);
        //EndPoint LocalEndPoint = (EndPoint)LocalIPEndPoint;
        //IPEndPoint remoteEP = (IPEndPoint)LocalEndPoint;
        //System.Diagnostics.Debug.WriteLine("Remote IP: " + remoteEP.Address.ToString());

        try
        {
            //all data should fit in one package!
            int bytesRead = receiveSocket.EndReceiveFrom(result, ref remoteEndPoint);
            //System.Diagnostics.Debug.WriteLine("Remote IP: " + ((IPEndPoint)(remoteEndPoint)).Address.ToString());

            byte[] bData = new byte[bytesRead];
            Array.Copy(recBuffer, bData, bytesRead);
            if (ByteHelper.isEndOfTransfer(bData))
            {
                System.Diagnostics.Debug.WriteLine("isEndOfTransfer");
                updateEndOfTransfer();// end of transfer
            }
            else if (ByteHelper.isMemInfoPacket(bData))
            {
                System.Diagnostics.Debug.WriteLine("isMemInfoPacket");
                try
                {
                    VMusage.MemoryInfoHelper mstat = new VMusage.MemoryInfoHelper();
                    mstat.fromByte(bData);
                    
                    //System.Diagnostics.Debug.WriteLine(mstat.ToString());

                    updateMem(mstat);
                }
                catch (Exception) { }
            }
            else if(ByteHelper.isLargePacket(bData)){
                System.Diagnostics.Debug.WriteLine("isLargePacket");
                try
                {
                    List<procVMinfo> lStats = new List<procVMinfo>();
                    VMusage.procVMinfo stats = new VMusage.procVMinfo();
                    lStats = stats.getprocVmList(bData, ((IPEndPoint)(remoteEndPoint)).Address.ToString());
                    updateStatusBulk(lStats);
                    //foreach (procVMinfo pvmi in lStats)
                    //{
                    //    pvmi.remoteIP = ((IPEndPoint)(remoteEndPoint)).Address.ToString();
                    //    //System.Diagnostics.Debug.WriteLine( stats.dumpStatistics() );
                    //}
                }
                catch (Exception) { }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("trying vmUsagePacket...");
                try
                {
                    VMusage.procVMinfo stats = new VMusage.procVMinfo(bData);
                    stats.remoteIP = ((IPEndPoint)(remoteEndPoint)).Address.ToString();
                    //System.Diagnostics.Debug.WriteLine( stats.dumpStatistics() );
                    if (stats.Time == 0)
                        stats.Time = DateTime.Now.Ticks;
                    updateStatus(stats);
                }
                catch (Exception) { }
            }
        }
        catch (SocketException e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("MessageReceivedCallback SocketException: {0} {1}", e.ErrorCode, e.Message));
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("MessageReceivedCallback Exception: {0}", e.Message));
        }
        try
        {
            //ready to receive next packet
            receiveSocket.BeginReceiveFrom(recBuffer, 0, recBuffer.Length, SocketFlags.None, ref bindEndPoint, new AsyncCallback(MessageReceivedCallback), (object)this);
        }
        catch (Exception) { }
    }

    public delegate void delegateUpdate(object sender, VMusage.procVMinfo data);
    public event delegateUpdate onUpdate;

    private void updateStatus(VMusage.procVMinfo data)
    {
        //System.Diagnostics.Debug.WriteLine("updateStatus: " + data.dumpStatistics());
        if (this.onUpdate != null)
            this.onUpdate(this, data);
    }

    public delegate void delegateUpdateBulk(object sender, List<procVMinfo> data);
    public event delegateUpdateBulk onUpdateBulk;

    private void updateStatusBulk(List<procVMinfo> data)
    {
        //System.Diagnostics.Debug.WriteLine("updateStatus: " + data.dumpStatistics());
        if (this.onUpdateBulk != null)
            this.onUpdateBulk(this, data);
    }

    public delegate void delegateUpdateMem(object sender, VMusage.MemoryInfoHelper data);
    public event delegateUpdateMem onUpdateMem;

    private void updateMem(MemoryInfoHelper data)
    {
        //System.Diagnostics.Debug.WriteLine("updateStatus: " + data.dumpStatistics());
        if (this.onUpdateMem != null)
            this.onUpdateMem(this, data);
    }

    public delegate void delegateEndOfTransfer(object sender, EventArgs e);
    public event delegateEndOfTransfer onEndOfTransfer;
    private void updateEndOfTransfer()
    {
        if (this.onEndOfTransfer != null)
            this.onEndOfTransfer(this, new EventArgs());
    }
    //#######################################################################################
   
}
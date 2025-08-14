using QuestDB;
using QuestDB.Senders;

namespace Aviator.Global.TimeSeries;

public class QuestDbClient(string connectionString)
{
    public ISender GetSender()
    {
        return Sender.New(connectionString);
    }
}
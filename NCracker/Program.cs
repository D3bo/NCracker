// See https://aka.ms/new-console-template for more information
using NBitcoin;
using System.Text.Json;

bool checkOnPrivateNode = false;
HttpClient client = new HttpClient();

if(!Directory.Exists("Data"))
{
    Directory.CreateDirectory("Data");
}

var walletFile = Path.Combine("Data", "wallet.json");

while (true)
{
    var seed = new NBitcoin.Mnemonic(Wordlist.English, WordCount.Twelve);
    var address = SeedToAddress(seed);
   var info = await GetBalance(address);
   Console.WriteLine($"Seed: {seed}; Address: {address}; Balance: {info.FinalBalance}; Tx Count: {info.TxCount}; Total Received: {info.TotalReceived}");

   if(info.FinalBalance > 0)
    AppendToWallet(seed.ToString(), info);
   
}

string SeedToAddress(Mnemonic seed)
{
    var seedBytes = seed.DeriveExtKey().PrivateKey.ToBytes();
    var privateKey = new Key(seedBytes);
    var publicKey = privateKey.PubKey;
    var address = publicKey.GetAddress(ScriptPubKeyType.SegwitP2SH, Network.Main);
    return address.ToString();
}

async Task<AddressInfo> GetBalance(string address)
{
    if (checkOnPrivateNode)
    {
        return new AddressInfo();
    }
    else
    {
        var response = await client.GetAsync($"https://blockchain.info/balance?active={address}");
        var result = await response.Content.ReadAsStringAsync();
       var doc = JsonDocument.Parse(result);
         var root = doc.RootElement;
            var addressElement = root.EnumerateObject().First();
            var balance = addressElement.Value.GetProperty("final_balance").GetInt64();
            var txCount = addressElement.Value.GetProperty("n_tx").GetInt32();
            var totalReceived = addressElement.Value.GetProperty("total_received").GetInt64();
            
            var addressInfo = new AddressInfo()
                
            {
                Address = address,
                FinalBalance = balance,
                TxCount = txCount,
                TotalReceived = totalReceived
            };
            return addressInfo;

    }
}

void AppendToWallet(string seed, AddressInfo info)
{
    info.Seed = seed;
    var address = JsonSerializer.Serialize(info);
    File.AppendAllText(walletFile, address + Environment.NewLine);
}

class AddressInfo
{
    public string Seed { get; set; }
    public string Address { get; set; }
    public Int64 FinalBalance { get; set; }
    public Int32 TxCount { get; set; }
    public Int64 TotalReceived { get; set; }
}


    







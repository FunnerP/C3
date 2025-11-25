using System.Security.Cryptography;

class Client
{
    private PaymentHandler handlerChain;

    public Client(PaymentHandler handlerChain)
    {
        this.handlerChain = handlerChain;
    }

    public void ProcessPayment(Payment payment)
    {
        Console.WriteLine($"Производится {payment.Type} платеж...");
        handlerChain.Handle(payment);
        Console.WriteLine();
    }
}
abstract class PaymentHandler
{
    protected PaymentHandler Handler;
    public void SetNext(PaymentHandler handler)
    {
        Handler = handler;
    }
    public virtual void Handle(Payment payment)
    {
        if (Handler != null)
            Handler.Handle(payment);
    }
}
class Payment
{
    public string Type { get; }
    public decimal Amount { get; }
    public bool IsProcess { get; set; } = false;
    public Payment(string type, decimal amount)
    {
        Type = type;
        Amount = amount;
    }
}
class LoggingHandler : PaymentHandler
{
    public override void Handle(Payment payment)
    {
        Console.WriteLine($"[Логирование] Платеж {payment.Amount} вида {payment.Type} начался.");
        base.Handle(payment);
    }
}
class ValidHandler : PaymentHandler
{
    public override void Handle(Payment payment)
    {
        if (payment.Amount <= 0)
        {
            Console.WriteLine("[Проверка] Сумма платежа должна быть положительной.");
            return;
        }
        Console.WriteLine("[Проверка] Платеж одобрен.");
        base.Handle(payment);
    }
}
class ComHandler : PaymentHandler
{
    private decimal Percent;

    public ComHandler(decimal feePercent)
    {
        this.Percent = feePercent;
    }

    public override void Handle(Payment payment)
    {
        decimal per = payment.Amount * Percent / 100;
        Console.WriteLine($"[Комиссия] Комиссия: {per:F2}");
        base.Handle(payment);
    }
}
class LgotiHandler : PaymentHandler
{
    private decimal nePercent; 
    public LgotiHandler(decimal NePercent)
    {
        this.nePercent = nePercent;
    }
    public override void Handle(Payment payment)
    {
        decimal lgoti = payment.Amount * nePercent / 100;
        Console.WriteLine($"[Льготы] Льготы сняты: {lgoti:F2}");
        base.Handle(payment);
    }
}
class GovHandler : PaymentHandler
{
    public override void Handle(Payment payment)
    {
        Console.WriteLine("[Государственный платеж] Государственный платеж.");
        base.Handle(payment);
    }
}
class InBankHandler : PaymentHandler
{
    public override void Handle(Payment payment)
    {
        Console.WriteLine("[Внутренний платеж] Внутренний платеж - без комиссии.");
        base.Handle(payment);
    }
}
class EndHandler : PaymentHandler
{
    public override void Handle(Payment payment)
    {
        payment.IsProcess = true;
        Console.WriteLine("[Конечный платеж] Конечный платеж совершен.");
    }
}



class Program
{
    static void Main()
    {
        // Обычные
        var BasePayChain = new LoggingHandler();
        var valid = new ValidHandler();
        var com = new ComHandler(5);
        var end = new EndHandler();

        BasePayChain.SetNext(valid);
        valid.SetNext(com);
        com.SetNext(end);

        // Льготные
        var lgoti = new LgotiHandler(10);
        loggingLgotiChainSetup();
        PaymentHandler lgotiChain;
        void loggingLgotiChainSetup()
        {
            var log = new LoggingHandler();
            var val = new ValidHandler();
            var lgt = new LgotiHandler(10);
            var com = new ComHandler(1);
            var end = new EndHandler();

            log.SetNext(val);
            val.SetNext(lgt);
            lgt.SetNext(com);
            com.SetNext(end);
            lgotiChain = log;
        }
        loggingLgotiChainSetup();

        // Государственные
        var govChain = new LoggingHandler();
        var govValid = new ValidHandler();
        var govRules = new GovHandler();
        var govCom = new ComHandler(0.5m);
        var govEnd = new EndHandler();

        govChain.SetNext(govValid);
        govValid.SetNext(govRules);
        govRules.SetNext(govCom);
        govCom.SetNext(govEnd);

        // Внутрибанковские
        var ibChain = new LoggingHandler();
        var ibValid = new ValidHandler();
        var ibInBank = new InBankHandler();
        var ibEnd = new EndHandler();

        ibChain.SetNext(ibValid);
        ibValid.SetNext(ibInBank);
        ibInBank.SetNext(ibEnd);

        // Клиенты 
        var normalClient = new Client(BasePayChain);
        var lgotiClient = new Client(lgotiChain);
        var govClient = new Client(govChain);
        var inBankClient = new Client(ibChain);

        // Тест
        normalClient.ProcessPayment(new Payment("Обычный", 1000));
        lgotiClient.ProcessPayment(new Payment("Льготный", 1000));
        govClient.ProcessPayment(new Payment("Государственный", 1000));
        inBankClient.ProcessPayment(new Payment("Внутренний", 1000));
    }
}
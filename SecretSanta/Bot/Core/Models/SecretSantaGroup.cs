namespace Bot.Core.Models;

internal class SecretSantaGroup
{
    private readonly List<ulong> _participants = [];
    private readonly List<ulong> _joinOrder = [];
    private readonly Dictionary<ulong, ulong> _recipientMap = new();
    private decimal _budget = 0;
    public decimal Budget
    {
        get => _budget;
        set
        {
            if (value >= 0)
            {
                _budget = value;
            }
        }
    }

    /// <summary>
    /// Shuffles the participants list in-place with the Fisher-Yates shuffle algorithm
    /// </summary>
    public void Shuffle()
    {
        var random = new Random();

        for (var i = _participants.Count - 1; i > 1; i--)
        {
            var rnd = random.Next(i + 1);

            (_participants[rnd], _participants[i]) = (_participants[i], _participants[rnd]);
        }
    }

    public void Assign()
    {
        for (var i = 0; i < _participants.Count; i++)
        {
            var currentSecretSanta = _participants[i];
            _recipientMap[currentSecretSanta] = _participants[(i + 1) % _participants.Count];
        }
    }

    public bool Join(ulong userId)
    {
        if (_participants.Contains(userId)) return false;
        _joinOrder.Add(userId);
        _participants.Add(userId);
        return true;
    }

    public bool Leave(ulong userId)
    {
        if (!_participants.Contains(userId)) return false;
        _joinOrder.Remove(userId);
        _participants.Remove(userId);
        return true;
    }
    public bool ValidSize() => _participants.Count >= 2;

    public List<ulong> List()
    {
        return _joinOrder;
    }

    public List<ulong> Participants()
    {
        return _participants;
    }

    public ulong? GetRecipient(ulong secretSantaId)
    {
        if (!_recipientMap.TryGetValue(secretSantaId, out var recipient))
        {
            return null;
        }
        return recipient;
    }
}
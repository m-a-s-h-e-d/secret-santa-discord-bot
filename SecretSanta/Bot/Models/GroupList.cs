using System.Reflection.Metadata.Ecma335;

namespace Bot.Models;

internal class SecretSantaGroup
{
  private readonly List<ulong> _participants = [];
  private readonly List<ulong> _joinOrder = [];
  private ulong? _firstParticipant = null;

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

    _firstParticipant = _participants.Count > 0 ? _participants[0] : null;
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

  public List<ulong> List()
  {
    return _joinOrder;
  }

  public List<ulong> Participants()
  {
    return _participants;
  }
}
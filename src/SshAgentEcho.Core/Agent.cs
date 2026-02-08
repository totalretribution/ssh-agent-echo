using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Renci.SshNet;
using SshNet.Agent;

namespace SshAgentEcho.Core;

public class Agent : IEnumerable<Agent.Identity>
{
    public readonly record struct Identity(string Comment, string User, string Host, string? Name, string Hash, string Type)
    {
        public override string ToString()
        {
            return $"{Type} {Hash} {Comment}";
        }
    }

    private readonly List<Identity> _identities = new();
    private int _cursor = -1;

    public Agent()
    {
        PopulateIdentities();
    }

    public void Refresh()
    {
        PopulateIdentities();
    }

    public void PrintIdentities()
    {
        foreach (var identity in _identities)
        {
            Console.WriteLine(identity);
        }
    }



    public IReadOnlyList<Identity> GetIdentities() => _identities.ToList();


    public Identity? Next()
    {
        if (_cursor + 1 >= _identities.Count) return null;
        _cursor++;
        return _identities[_cursor];
    }

    public void ResetIteration() => _cursor = -1;

    public IEnumerator<Identity> GetEnumerator() => _identities.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private String? GetOpenSshKey(SshAgentPrivateKey? identity)
    {
        if (identity?.Key == null) return null;
        var key = identity.Key;

        var keyDataProperty = key.GetType().GetProperty("KeyData");
        if (keyDataProperty == null) return null;

        var keyValue = keyDataProperty.GetValue(key);
        if (keyValue == null) return null;
        var publicKeyBlob = keyValue as byte[];
        if (publicKeyBlob == null) return null;
        if (publicKeyBlob.Length == 0) return null;

        return Convert.ToBase64String(publicKeyBlob);
    }

    private string? ExtractChevronContent(string input)
    {
        int start = input.IndexOf('<');
        int end = input.IndexOf('>');
        if (start >= 0 && end > start)
        {
            return input.Substring(start + 1, end - start - 1);
        }
        return null;
    }

    private (string User, string Host, string? Name)? ProcessComment(string comment)
    {
        string? user_host = comment.Trim();
        string user = "";
        string host = "";
        string? name = null;
        if (comment.Contains('<') || comment.Contains('>'))
        {
            // Must have exactly one '<' and one '>'
            if (comment.Count(c => c == '<') != 1 || comment.Count(c => c == '>') != 1)
            {
                return null;
            }
            // '>' must come after '<'
            if (comment.IndexOf('<') > comment.IndexOf('>'))
            {
                return null;
            }
            // '<' must not be the first character, the name must be at least 2 characters long.
            if (comment.IndexOf('<') < 2)
            {
                return null;
            }
            user_host = ExtractChevronContent(comment);
            if (string.IsNullOrEmpty(user_host)) return null;
            // validation omitted for brevity...
            int idx = comment.IndexOf('<');
            if (idx >= 0)
                name = comment.Substring(0, idx).Trim().Replace(" ", "_");
        }

        if (user_host.Count('@') != 1) return null;
        if (user_host.IndexOfAny(new[] { ' ', '\t', '\n', '\r', '\v', '\f' }) >= 0) return null;

        string[] parts = user_host.Split('@');
        if (parts.Length != 2) return null;
        if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1])) return null;
        user = parts[0];
        host = parts[1];
        return (user, host, name);
    }

    private void PopulateIdentities()
    {
        this._identities.Clear();
        _cursor = -1;
        try
        {
            var agent = new SshAgent();
            var agentIdentities = agent.RequestIdentities();
            if (agentIdentities == null) return;

            foreach (var id in agentIdentities)
            {
                var hash = GetOpenSshKey(id);
                var type = id?.Key?.ToString();
                var comment = id?.Key.Comment;
                if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(comment))
                    continue;
                var process_comment = ProcessComment(comment);
                if (process_comment == null)
                    continue;

                var (user, host, name) = process_comment.Value;
                var identity = new Identity(Comment: comment, User: user, Host: host, Name: name, Hash: hash, Type: type);
                this._identities.Add(identity);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error querying SSH agent: {ex.Message}");
        }
    }
}
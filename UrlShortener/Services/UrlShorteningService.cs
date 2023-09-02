﻿using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Services;

public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string Alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
    private readonly Random _random = new();
    private readonly ApplicationDbContext _context;

    public UrlShorteningService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateUniqueCode()
    {
        var codeChars = new char[NumberOfCharsInShortLink];

        while (true)
        {
            for (var i = 0; i < NumberOfCharsInShortLink; i++)
            {
                int randomIndex = _random.Next(Alphabet.Length - 1);
                codeChars[i] = Alphabet[randomIndex];
            }

            var code = new string(codeChars);

            if (!await _context.ShortenedUrls.AnyAsync(s => s.Code == code))
            {
                return code;
            }
        }


    }
}
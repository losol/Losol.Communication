﻿using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Losol.Communication.Email.File
{
	/// <summary>
	/// Writes an email to a file instead of actually sending it.
	/// This implementation is not designed to be used in production.
	/// </summary>
	public class FileEmailWriter : IEmailSender
    {
        private readonly IOptions<FileEmailConfig> _options;

        public FileEmailWriter(IOptions<FileEmailConfig> options)
        {
            _options = options;
            if (!Directory.Exists(options.Value.FilePath))
            {
                Directory.CreateDirectory(options.Value.FilePath);
            }
        }

        public async Task SendEmailAsync(
            string address,
            string subject,
            string message,
            Attachment attachment = null,
            EmailMessageType messageType = EmailMessageType.Html)
        {
            // filename: {datetime}-{email}-{subject}.html
            var filename = $"{DateTime.Now:yyyyMMdd-HHmmss}-{address.GenerateSlug()}-{subject.GenerateSlug()}.html";

            // Write the message to the file
            await using var outputFile = new StreamWriter(Path.Combine(_options.Value.FilePath, filename));
            await outputFile.WriteLineAsync(message);
        }

        public Task SendEmailAsAsync(string fromName, string fromEmail, string address, string subject, string message,
            Attachment attachment = null, EmailMessageType messageType = EmailMessageType.Html)
        {
            return SendEmailAsync(address, subject, message, attachment, messageType);
        }
    }

    internal static class StringExtensions
    {
		/// <summary>
		/// Creates a URL And SEO friendly slug
		/// Copyright (c) Johan Boström. All rights reserved.
		/// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
		/// </summary>
		/// <param name="text">Text to slugify</param>
		/// <param name="maxLength">Max length of slug</param>
		/// <returns>URL and SEO friendly string</returns>
		internal static string GenerateSlug(this string text, int maxLength = 0)
		{
			// Return empty value if text is null
			if (text == null) return "";

			var normalizedString = text
				.ToLowerInvariant()
				.Normalize(NormalizationForm.FormD);

			var stringBuilder = new StringBuilder();
			var stringLength = normalizedString.Length;
			var prevdash = false;
			var trueLength = 0;

			char c;

			for (int i = 0; i < stringLength; i++)
			{
				c = normalizedString[i];

				switch (CharUnicodeInfo.GetUnicodeCategory(c))
				{
					// Check if the character is a letter or a digit if the character is a
					// international character remap it to an ascii valid character
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.UppercaseLetter:
					case UnicodeCategory.DecimalDigitNumber:
						if (c < 128)
							stringBuilder.Append(c);
						else
							stringBuilder.Append(RemapInternationalCharToAscii(c));

						prevdash = false;
						trueLength = stringBuilder.Length;
						break;

					// Check if the character is to be replaced by a hyphen but only if the last character wasn't
					case UnicodeCategory.SpaceSeparator:
					case UnicodeCategory.ConnectorPunctuation:
					case UnicodeCategory.DashPunctuation:
					case UnicodeCategory.OtherPunctuation:
					case UnicodeCategory.MathSymbol:
						if (!prevdash)
						{
							stringBuilder.Append('-');
							prevdash = true;
							trueLength = stringBuilder.Length;
						}
						break;
				}

				// If we are at max length, stop parsing
				if (maxLength > 0 && trueLength >= maxLength)
					break;
			}

			// Trim excess hyphens
			var result = stringBuilder.ToString().Trim('-');

			// Remove any excess character to meet maxlength criteria
			return maxLength <= 0 || result.Length <= maxLength ? result : result.Substring(0, maxLength);
		}

		/// <summary>
		/// Remaps international characters to ascii compatible ones
		/// Copyright (c) Johan Boström. All rights reserved.
		/// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
		/// </summary>
		/// <param name="c">Character to remap</param>
		/// <returns>Remapped character</returns>
		internal static string RemapInternationalCharToAscii(char c)
		{
			string s = c.ToString().ToLowerInvariant();
			if ("àåáâäãåą".Contains(s))
			{
				return "a";
			}
			else if ("èéêëę".Contains(s))
			{
				return "e";
			}
			else if ("ìíîïı".Contains(s))
			{
				return "i";
			}
			else if ("òóôõöøőð".Contains(s))
			{
				return "o";
			}
			else if ("ùúûüŭů".Contains(s))
			{
				return "u";
			}
			else if ("çćčĉ".Contains(s))
			{
				return "c";
			}
			else if ("żźž".Contains(s))
			{
				return "z";
			}
			else if ("śşšŝ".Contains(s))
			{
				return "s";
			}
			else if ("ñń".Contains(s))
			{
				return "n";
			}
			else if ("ýÿ".Contains(s))
			{
				return "y";
			}
			else if ("ğĝ".Contains(s))
			{
				return "g";
			}
			else if (c == 'ř')
			{
				return "r";
			}
			else if (c == 'ł')
			{
				return "l";
			}
			else if (c == 'đ')
			{
				return "d";
			}
			else if (c == 'ß')
			{
				return "ss";
			}
			else if (c == 'þ')
			{
				return "th";
			}
			else if (c == 'ĥ')
			{
				return "h";
			}
			else if (c == 'ĵ')
			{
				return "j";
			}
			else
			{
				return "";
			}
		}
    }
}

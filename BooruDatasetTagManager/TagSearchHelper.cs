using System;
using System.Collections.Generic;

namespace BooruDatasetTagManager
{
    public readonly struct TagSearchItem
    {
        public string Tag { get; }
        public string Translation { get; }

        public TagSearchItem(string tag, string translation = null)
        {
            Tag = tag ?? string.Empty;
            Translation = translation ?? string.Empty;
        }
    }

    public static class TagSearchHelper
    {
        public static int FindBestMatch(
            IReadOnlyList<TagSearchItem> items,
            string query,
            int startIndex,
            bool forward = true,
            bool matchCase = false,
            bool wholeWord = false,
            ISet<string> aliasTags = null)
        {
            if (items == null)
                return -1;
            return FindBestMatch(items.Count, i => items[i], query, startIndex, forward, matchCase, wholeWord, aliasTags);
        }

        public static int FindBestMatch(
            int count,
            Func<int, TagSearchItem> getItem,
            string query,
            int startIndex,
            bool forward = true,
            bool matchCase = false,
            bool wholeWord = false,
            ISet<string> aliasTags = null)
        {
            if (getItem == null || count <= 0 || string.IsNullOrWhiteSpace(query))
                return -1;

            query = query.Trim();
            StringComparison comp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            startIndex = ((startIndex % count) + count) % count;

            int containsMatch = -1;
            int translationMatch = -1;
            int aliasMatch = -1;

            for (int offset = 0; offset < count; offset++)
            {
                int i = forward
                    ? (startIndex + offset) % count
                    : ((startIndex - offset) % count + count) % count;

                TagSearchItem item = getItem(i);
                string tag = item.Tag;
                string translation = item.Translation;

                if (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(translation))
                    continue;

                if (wholeWord)
                {
                    if (string.Equals(tag, query, comp))
                        return i;
                    if (!string.IsNullOrEmpty(translation) && string.Equals(translation, query, comp))
                        return i;
                    if (aliasTags != null && aliasTags.Contains(tag))
                        return i;
                    continue;
                }

                // Exact match has highest priority
                if (string.Equals(tag, query, comp))
                    return i;

                // Prefix match on tag
                if (tag.StartsWith(query, comp))
                    return i;

                if (containsMatch == -1 && tag.Contains(query, comp))
                    containsMatch = i;

                if (translationMatch == -1 && !string.IsNullOrEmpty(translation) && translation.Contains(query, comp))
                    translationMatch = i;

                if (aliasMatch == -1 && aliasTags != null && aliasTags.Contains(tag))
                    aliasMatch = i;
            }

            if (containsMatch != -1)
                return containsMatch;
            if (translationMatch != -1)
                return translationMatch;
            return aliasMatch;
        }
    }
}

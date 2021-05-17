Available display settings
Configure number of data displayed in single request. All commands and values are case insensitive. All EPGP settings are handled by DKP configurations.
Multiple players DKP Display
Category: dkp

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |    16 | from 1 to 32
fields                   |     6 | from 1 to 16
multiple-columns         | True  | True False
separate-messages        |     5 | from 0 to 16
value-suffix             | True  | True False
Player DKP history
Category: dkp-history

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |    10 | from 1 to 32
fields                   |     1 | from 1 to 16
multiple-columns         | True  | True False
separate-messages        |     1 | from 0 to 16
value-suffix             | True  | True False
Player loot history
Category: loot-history

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |    10 | from 1 to 32
fields                   |     1 | from 1 to 16
multiple-columns         | True  | True False
separate-messages        |     1 | from 0 to 16
value-suffix             | True  | True False
Latest raid loot
Category: latest-loot

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |     5 | from 1 to 32
fields                   |     6 | from 1 to 16
multiple-columns         | False | True False
separate-messages        |     1 | from 0 to 16
value-suffix             | True  | True False
Item search results
Category: item-search

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |     5 | from 1 to 32
fields                   |     6 | from 1 to 16
multiple-columns         | False | True False
separate-messages        |     3 | from 0 to 16
value-suffix             | True  | True False
Item value results
Category: item-value

alternative-display-mode | False | True False
enable-icons             | True  | True False
entries-per-field        |     5 | from 1 to 32
fields                   |     6 | from 1 to 16
multiple-columns         | False | True False
separate-messages        |     3 | from 0 to 16
value-suffix             | True  | True False
​
Usage:

!display Category Config Value
Example:

!display loot-history multiple-columns True
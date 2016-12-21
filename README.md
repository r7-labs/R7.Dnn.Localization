# About R7.Dnn.Localization

[![Join the chat at https://gitter.im/roman-yagodin/R7.Dnn.Localization](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/roman-yagodin/R7.Dnn.Localization?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

*R7.Dnn.Localization* project goal is to provide a possibly complete russian translations for [DNN Plaftorm](http://www.dnnsoftware.com/) Core and some widely used extensions.

![Localized Admin &gt; Languages view](https://raw.githubusercontent.com/roman-yagodin/R7.Dnn.Localization/master/images/admin_languages.png)

## Status

- DNN v8.0.4 Core (`Core8` folder) - 100% complete.
- DNN v7.4.2 Core (`Core7` folder) - about 98% complete.
- Blog module - 100% complete.
- Feedback module - 100% complete, in review.
- Forum module - about 49% complete (average user UI).
- CKEditor HTML editor provider - 100% complete, accepted in [original project](https://github.com/w8tcha/dnnckeditor).

Source files for DNN v7.1.2 available in [dnn-712 branch](https://github.com/roman-yagodin/R7.Dnn.Localization/tree/dnn-712).

## Install

Get the latest *Core* language pack from [releases](https://github.com/roman-yagodin/R7.Dnn.Localization/releases),
then go to *Host &gt; Extensions* and install like any other extension. After that, you should see *Russian*
language available to select in Admin &gt; Languages.

Language packs for *Blog*, *Forum* and *Feedback* modules and *CKEditor HTML editor provider* should be installed separately, 
the same way as for core one.

## Contribute

You could contribute your translations and fixes
via [Transifex](https://www.transifex.com/labs-r7/public/) or directly via `git` pull requests.

## Customize

Though currently I don't have plans for translating *DNN* to any other language than russian, 
all helper scripts are configurable and customizable. So feel free to extend the project by providing 
translation for other cultures!

The easiest way to start is to request new translation language 
for one of DNN-related [Transifex projects](https://www.transifex.com/labs-r7/public/) 
and (optionally) fork *R7.Dnn.Localization* project on GitHub. 
From that point we could work together to support new language - if you really up to it.

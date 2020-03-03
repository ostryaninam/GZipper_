README
Общее описание:
Программа сжимает файл, используя несколько потоков. Исходный файл разбивается на блоки размером 1 Мб, которые сжимаются в нескольких потоках при помощи класса GZipStream, и записываются в целевой файл с расширением .gz. 
Алгоритм работы с блоками осуществлен с использованием паттерна Producer-Consumer: блоки файла считываются из файла, добавляются в коллекцию, откуда их забирают потоки, осуществляющие сжатие/расжатие. В свою очередь, обработанные блоки добавляются в другую очередь, откуда их забирает пишушщий в файл поток.
Инструкции:
Для работы с программой запустите из командной строки GZipTest.exe, лежащий в папке GZipTest. Формат ввода: GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла], имена файлов без кавычек.
В настоящее время проект находится в процессе доработки.

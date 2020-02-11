README
Общее описание:
Программа сжимает файл, используя несколько потоков. Исходный файл разбивается на блоки размером 1 Мб, которые сжимаются в нескольких потоках при помощи класса GZipStream, и записываются в целевой файл с расширением .gz. Программа умеет "разжимать" файл, т.е. осуществлять обратный процесс обработки сжатого файла.
1) Библиотека классов	FileManagerLibrary содержит интерфейсы и их реализации классов, обрабатывающего файлы. Класс должен уметь считывать и записывать блок байт в соответствие с форматом файла (сжатый/обычный). 
2)	GZip содержит абстрактный класс GZipper, описывающий стандарт для классов GZipCompressor и GZipDecompressor. 
3) FixedThreadLibrary представляет из себя простую реализацию ThreadPool ( в условии задачи не разрешается использовать библиотечный ThreadPool).
Обработка ошибок осуществлена с выводом сообщения в консоль и завершением приложения. 
Алгоритм работы с блоками осуществлен с использованием паттерна Producer-Consumer: обработанные блоки складываются в соответствующие коллекции (см. библиотеку классов BlockingCollection),
откуда их "забирает" поток, пишущий в файл.
Инструкции:
Для работы с программой запустите из командной строки GZipTest.exe, лежащий в папке GZipTest. Формат ввода: GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла], имена файлов без кавычек.
В настоящее время проект находится в процессе доработки.

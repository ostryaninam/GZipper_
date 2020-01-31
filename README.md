README
Общее описание:
Программа сжимает файл, используя несколько потоков. Исходный файл разбивается на блоки размером 1 Мб, которые сжимаются в нескольких потоках при помощи класса GZipStream, и записываются в целевой файл с расширением .gz. Программа умеет "разжимать" файл, т.е. осуществлять обратный процесс обработки сжатого файла.
1) Библиотека классов	FileManagerLibrary содержит интерфейс класса, обрабатывающего файлы. Класс должен уметь считывать и записывать блок байт в соответствие с форматом файла (сжатый/обычный). 
При записи в сжатый файл в начало каждого блока записывается его длина, в начало файла записывается общее количество блоков. При чтении сначала читается длина блока, затем сам блок.
2)	GZip содержит абстрактный класс GZipper, описывающий общие для сжатия и распаковки инструкции, и два класса, наследующих GZipper- GZipCompressor и GZipDecompressor. Последние два класса задают нужную операцию (CompressBlock/DecompressBlock).
3) FixedThreadLibrary представляет из себя простую реализацию ThreadPool ( в условии задачи не разрешается использовать библиотечный ThreadPool).
Обработка ошибок осуществлена с выводом сообщения в консоль и завершением приложения. Эта часть нуждается в доработке, так как правильным вариантом было бы “прокидывание” ошибок в верхние слои логики.
Алгоритм работы с несколькими потоками, сжимающими/разжимающими блоки файла, выглядит следующим образом:
1)	Создается количество потоков по количеству логических процессоров
2)	Задается максимальный размер коллекции для сжатых/распакованных блоков (Dictionary)
3)	Каждый поток под блокировкой считывает блок файла, сжимает/разжимает его. 
4)	Пишущий в файл поток идет по порядку по ключам и записывает готовые блоки в файл. Если блок с текущим номером еще не в коллекции, поток ждет сообщения о поступлении в колекцию нового блока.
Инструкции:
Для работы с программой запустите из командной строки GZipTest.exe, лежащий в папке GZipTest. Формат ввода: GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла], имена файлов без кавычек.
В настоящее время проект находится в процессе доработки.

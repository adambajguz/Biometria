# Biometria

# Opis zadanie małe nr 1
Przygotuj program do przetwarzania plików graficznych (w wybranym języku programowania). Przez kolejne zajęcia będziesz rozbudowywał ten program, aby dodać do niego kolejne opcje - pamiętaj o tym przy projektowaniu. Program powinien mieć graficzny interfejs użytkownika i oferować następujące funkcjonalności:

* Wczytaj i wyświetl wskazany przez użytkownika plik graficzny, obsługiwane formaty to jpeg, png, bmp, gif, tiff. 
* Wyświetl obraz w powiększeniu (np. x8)
* Odczytaj wartość RGB wybranego piksela (np. wskazanego myszką).
* Zmień wartość wskazanego piksela (np. po kliknięciu)
* Zapisz plik wynikowy pod wskazaną nazwą i we wskazanym formacie


# Zadanie "małe" nr 2
Uzupełnij program z poprzednich zajęć o możliwość obliczenia i zaprezentowania (w postaci wykresu) histogramu wczytanego obrazu. W wypadku obrazów barwnych, oblicz osobne histogramy dla kanałów R, G i B oraz histogram dla wartości uśrednionych (R+G+B)/3. Następnie wykonaj operacje wykorzystujące LUT (lookup table):

* rozjaśnienie i przyciemnienie przy użyciu funkcji logarytmicznych lub kwadratowych
* rozciągnięcie histogramu (dla podanych wartości jasności a i b, ucinane są wartości poniżej a i powyżej b, a pozostała część rozciągana do pełnego zakresu jasności)
* wyrównanie histogramu (operacja przy użyciu histogramu skumulowanego jako LUT)
* Aplikacja powinna umożliwiać zapis przekształconych obrazów.


# Zadanie "małe" nr 3

Uzupełnij program z poprzednich zajęć o możliwość wykonywania binaryzacji (progowania). Obrazy kolorowe przed operacją binaryzacji zamień na postać w skali szarości.
* Binaryzacja z ręcznie wyznaczonym progiem - pozwól użytkownikowi wybrać wartość progu.
* Automatyczne wyznaczanie progu - wyznacz próg przy pomocy metody Otsu. W metodzie Otsu próg wyznaczany jest przez minimalizację wariancji wewnątrzklasowej (lub maksymalizację wariancji międzyklasowej).
* Binaryzacja lokalna - wykorzystując metodę Niblacka. W metodzie Niblacka prog wyznaczany jest osobno dla każdego piksela na podstawie średniej i wariancji wartości pikseli w jego otoczeniu. Pozwól użytkownikowi wybrać rozmiar okna oraz wartość k (parametr progowania).

Pomoce:
* Wykład o progowaniu (autor: dr inż. Marcin Wilczewski, Politechnika Gdańska)
* Fragmenty ksiażki "Cyfrowe przetwarzanie obrazów" (W. Malina, M. Smiatacz, Akademicka Oficyna Wydawnicza EXIT, Warszawa 2008)


# Zadanie "małe" nr 4

Dodaj do programu możliwość filtrowania obrazu:

* Przy pomocy filtrów liniowych opartych o funkcję splotu (filtry konwolucyjne). Wykorzystaj maskę 3x3, dając uzytkownikowi możliwość wprowadzenia jej wartości. Przetestuj działanie na kilku standardowych operatorach: rozmywającym (filtr dolnoprzepustowy), Prewitta, Sobela, Laplace'a, wykrywający narożniki.
* Przy pomocy filtru Kuwahara.
* Przy pomocy filtru medianowego (dla masek 3x3 i 5x5).

# Zadanie projektowe/zespołowe nr 1 (Fingerprint)

Mając dany zbiór obrazów odcisków palca  należy wykonać następujące zadania:

* Binaryzacja obrazu (o ile jest niezbędna) - w tym przypadku mogą Państwo skorzystać z już zaimplementowanych metod.
* (2 pkt/20 pkt w CEZ) Szkieletyzacja (ścienianie) odcisku palca - wynikiem tej procedury powinna być reprezentacja odcisku palca w formie linii o szerokości jednego piksela (przykładem jest obraz 01t.png).
Opisy algorytmów szkieletyzacji z których należy skorzystać znajdą Państwo w folderze "Materiały pomocnicze do zadania Fingerprint".
Zalecanymi algorytmami do wykorzystania są: KMM albo jego późniejsza modyfikacja K3M.

Druga część zadania polega na odnalezieniu punktów charakterystycznych (minucji) odcisku palca i oznaczeniu ich na obrazie. Te części będą punktowane w sposób następujący:

* (1 pkt/10 pkt w CEZ) Odnalezienie rozgałęzień.
* (1 pkt/10 pkt w CEZ) Przefiltrowanie wykrytych punktów charakterystycznych - odrzucenie fałszywych minucji (tj. znajdujących się na krawędziach odcisku palca albo położonych zbyt blisko siebie).

Zbiór obrazów na których należy testować przygotowane rozwiązanie. Obrazy 01-10 są obrazami zawierającymi nieścienione odciski palców, natomiast obraz 01t zawiera przykładowy wynik, który powinien zostać zwrócony przez algorytm.

https://github.com/abdelrahmanMo/Fingerprint-Recognition

# Zadanie projektowe/zespołowe nr 2 (Keystroke Dynamics)
Państwa zadaniem jest przygotowanie pełnego systemu biometrycznego w oparciu o analizę sposobu  pisania na klawiaturze. Do tego celu niezbędne będzie poznanie metody klasyfikacji jaką jest k-Najbliższych Sąsiadów oraz metryk: Euklidesa, Manhattan oraz Czebyszewa.

Ocenie podlegają następujące elementy:

(1pkt/ 10pkt na CEZ2) Możliwość zbierania próbek z użyciem dwell time (oraz poprawność wykonania tego elementu) oraz zapis wektorów do bazy danych. Wektor cech powinien zawierać średni czas wciśnięcia dla każdej litery angielskiego alfabetu oraz informacje o kategorii obiektu.
(2pkt/ 20pkt na CEZ2) Wykonane badania dotyczące jakości systemu. W tym miejscu muszą Państwo zastosować metodę k-Najbliższych Sąsiadów (metoda leave-one-out) oraz trzy wymienione wcześniej metryki.
(0,5pkt/ 5pkt na CEZ2) Jakość przygotowanego sprawozdania oraz opracowane wnioski.
(0,5pkt/ 5pkt na CEZ2) Możliwość wykonania identyfikacji oraz weryfikacji.
Dodatkowe punkty mogą Państwo zdobyć za:

(0,5pkt/ 5pkt na CEZ2) Obliczenie jakości systemu przy zastosowaniu metryki Mahalanobisa.
(0,5pkt/ 5pkt na CEZ2) Wykorzystanie w systemie czasu flight time.
Wynikiem Państwa pracy muszą być dwa elementy:

Program, który zostanie przez Państwa umieszczony na platformie CEZ2. Powinien on zawierać następujące moduły: zbierania danych, weryfikacji oraz identyfikacji.
Sprawozdanie, które będzie zawierało elementy następujące: krótki opis systemu (informacja o decyzji jaki czas jest badany oraz krótkie uzasadnienie); wykresy dotyczące jakości dla poszczególnych metryk (przy różnych wartościach parametru k); wyniki analizy porównawczej z programem, który jest dostępny na platformie CEZ2 oraz wnioski dotyczące wykonanych eksperymentów. Sprawozdanie nie powinno przekroczyć 4 stron.

===
ma być okno jak logowanie tylko że z jednym pole tekstowym
quikc brown fox - użyć po coś bo pokrywa wszystkie litery
function [x] = myfunc(a,b,c) 

h = figure(1);
set(h, 'Visible', 'off');
h.PaperUnits = 'inches';
h.PaperPosition = [0 0 18 4];

x = plot(a,b,a,c);

title 'Grafstjärtis';
xlabel 'TidJävel';
ylabel 'Vinkel';

saveas(h, 'VinkelgrafJÄVEL.jpeg')

end

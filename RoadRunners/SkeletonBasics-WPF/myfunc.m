function [x] = myfunc(a,b,c,d) 
% a = Tid 
% b = puls
% c = vinklar_FHK
% d = vinklar_SHK
h = figure(1); set(gcf,'visible','off')

h.PaperUnits = 'inches';
h.PaperPosition = [0 0 16 3];



x = plot (a,b,a,c,a,d);
% aa=[a;a];
% bb=[b;b];
% zz=zeros(size(aa));
% 
% hs=surf(aa,bb,zz,bb, 'EdgeColor', 'interp')
% colormap(flipud(autumn))
% view(2)
% 
% HANDLE = gca;
% get(HANDLE);
% set(HANDLE, 'Color', [0,0.1,0.3]);
% hs.LineWidth=2
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');
 legend('Puls','Vinklar FHK','Vinklar SHK' , 'location', 'southwest');

saveas(h, 'Vinkelgraf.jpeg')
close(h);
end



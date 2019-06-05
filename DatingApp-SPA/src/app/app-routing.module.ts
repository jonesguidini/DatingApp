import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListsComponent } from './lists/lists.component';
import { AuthGuard } from './_guards/auth.guard';

const routes: Routes = [
    {path: '', component: HomeComponent},
    {
      path: '',
      runGuardsAndResolvers: 'always',
      canActivate: [AuthGuard],
      children: [
        {path: 'members', component: MemberListComponent},
        {path: 'messages', component: MessagesComponent},
        {path: 'lists', component: ListsComponent}
      ]
    },
    {path: '**', redirectTo: '', pathMatch: 'full'}
    // em caso nenhuma das opções abaixo seja encontrada (path) é direcionado para 'home'
    /* IMPORTANTE - A ordem dos mapeamentos das rotas é importante por isso o curinga '**' é o ultimo */
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

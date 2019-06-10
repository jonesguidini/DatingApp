import { Injectable } from "@angular/core";
import { CanDeactivate } from '@angular/router';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable()

// esse GUARD é usada para controlar as paginas que contém formulário alterado mas não salvo
// *** verificar no arquivo de 'routings' a injeção do canDeactivate para utilização desse GUARD

export class PreventUnsavedChanges implements CanDeactivate<MemberEditComponent> {
    canDeactivate(component: MemberEditComponent) {
        if (component.editForm.dirty) {
            return confirm('Are you shure you want to continue? Any unsaved changes will be lost!');
        }

        return true;
    }
}